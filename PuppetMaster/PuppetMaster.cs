using MapWorker;
using PADIMapInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClientLib;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace PuppetMaster
{
    public class PuppetMaster : MarshalByRefObject, IPuppetMaster
    {
        #region Class Variables
        SortedList<int,String> workersURL;
        int portCounter;
        public delegate int RemoteStatusAsyncDelegate();
        public delegate void RemoteSlowDelegate(int sec);
        Dictionary<string, IWorker> mapProxies = new Dictionary<string, IWorker>();
        Dictionary<string, IPuppetMaster> puppetProxies = new Dictionary<string, IPuppetMaster>();

        #endregion

        #region Constructor
        public PuppetMaster()
        {
            workersURL = new SortedList<int, String>();
            portCounter = 10001;
        }
        public override object InitializeLifetimeService()
        {

            return null;

        }
        #endregion

        private IWorker getRemoteWorker(string objUrl)
        {
            if (mapProxies.ContainsKey(objUrl))
                return mapProxies[objUrl];
            else
            {
                IWorker newWorker = (IWorker)Activator.GetObject(typeof(IWorker), objUrl);
                mapProxies.Add(objUrl, newWorker);
                return newWorker;
            }
        }

        private IPuppetMaster getRemotePM(string objUrl)
        {
            if (puppetProxies.ContainsKey(objUrl))
                return puppetProxies[objUrl];
            else
            {
                IPuppetMaster newPM = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), objUrl);
                puppetProxies.Add(objUrl, newPM);
                return newPM;
            }
        }

        // Compute Command Method
        public bool computeCommand(string cmd)
        {
            string[] splits = cmd.Split(null); //Splits strings by spaces

            if (splits[0] == "") return true;
            if (splits[0][0] == '%' || splits[0][0] == ' ') return true;
            if (splits[0].Equals("WORKER"))
            {
                /*WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>: Contacts the PuppetMaster
                at the PUPPETMASTER-URL to creates a worker process with an identifier <ID>
                that exposes its services at <SERVICE-URL>. If an <ENTRY-URL> is provided, the new
                worker should notify the set of existing workers that it has started by calling the worker
                listening at <ENTRY-URL>. Since this command can be used to create local or remote
                workers, it will be simpler to implement it as a call to the local (or remote) PuppetMaster’s
                job creation service.*/

                int id = Int32.Parse(splits[1]);
                string puppetMasterURL = splits[2];
                string workerURL = splits[3];

                IPuppetMaster puppetMasterProxy = getRemotePM(puppetMasterURL);
                if (puppetMasterProxy == null) return false;

                if (splits.Length == 4) puppetMasterProxy.CreateWorker(id, workerURL);
                else puppetMasterProxy.CreateWorkerEntry(id, workerURL, splits[4]);

                if (workersURL.ContainsKey(id))
                    workersURL.Remove(id);
                workersURL.Add(id, workerURL);
            }

            else if (splits[0].Equals("SUBMIT"))
            {
                /*SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> <DLL>: Creates an application on the
                local node. The application submits a job to the PADIMapNoReduce platform by
                system by contacting the worker at <ENTRY-URL>. The job is defined by the following
                parameters:
                – <FILE> is the path to the input file. The file will be subdivided into <S> splits
                across the machines in W.
                – <OUTPUT> is the path to an output directory on the local filesystem of the application,
                which will store one output file for each split of the input file name “1.out”,
                “2.out”, . . ., “n.out”.
                – <S>, i.e. the number of splits of the input file, which corresponds to the total
                number of worker tasks to be executed.
                – The name of the class implementing the IMap interface.*/

                string entryURL = splits[1];
                string filePath = splits[2];
                string outputPath = splits[3];
                int slices = Int32.Parse(splits[4]);
                string className = splits[5];
                string dllPath = splits[6];
                int clientPort = portCounter++;

                ProcessStartInfo startInfo = new ProcessStartInfo("UserLevelApp.exe");
                startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                startInfo.Arguments = this.parseIP(entryURL) + " " + this.parsePort(entryURL) + " " + outputPath + " " + filePath + " " + dllPath + " " + className + " " + slices + " " + clientPort;

                Process.Start(startInfo);

            }

            else if (splits[0].Equals("WAIT"))
            {
                /*WAIT <SECS>: Makes the PuppetMaster stop the execution of commands of the script
                for <SECS> seconds*/

                Thread.Sleep(Int32.Parse(splits[1]) * 1000);
            }

            else if (splits[0].Equals("STATUS"))
            {
                /*STATUS: Makes all workers and job trackers of the PADIMapNoReduce system print
                their current status. The report shall allow to determine the state of progress of each
                map task at every node of the platform, as well as the current phase of execution of
                the job (e.g., transfer of the input/output data, computing, etc.). The status command
                should present brief information about the state of the system (who is present, who is
                in charge of coordination, which nodes are presumed failed). Status information can be
                printed on each nodes’ console and does not need to be centralized at the PuppetMaster.
                */

                foreach(KeyValuePair<int, string> kvp in workersURL){
                    try {
                        RemoteStatusAsyncDelegate RemoteDel = new RemoteStatusAsyncDelegate(getRemoteWorker(kvp.Value).TrackJob);
                        RemoteDel.BeginInvoke(null, null);
                    }
                    catch(Exception){
                        Console.WriteLine("Worker at " + kvp.Value + " doesnt respond");
                    }
                }
            }

            else if (splits[0].Equals("SLOWW"))
            {
                /*SLOWW <ID> <delay-in-seconds>: Injects the specified delay in the worker processes
                with the <ID> identifier*/
                int id = Int32.Parse(splits[1]);
                String workerURL = workersURL.ElementAt(workersURL.IndexOfKey(id)).Value;
                getRemoteWorker(workerURL).Slow(Int32.Parse(splits[2]));
            }

            else if (splits[0].Equals("FREEZEW"))
            {
                /*FREEZEW <ID>: Disables the communication of a worker and pauses its map computation
                in order to simulate the worker’s failure*/

                int id = Int32.Parse(splits[1]);
                String workerURL = workersURL.ElementAt(workersURL.IndexOfKey(id)).Value;
                getRemoteWorker(workerURL).FreezeW();
            }

            else if (splits[0].Equals("UNFREEZEW"))
            {
                /*UNFREEZEW <ID>: Undoes the effects of a previous FREEZEW command.*/

                int id = Int32.Parse(splits[1]);
                String workerURL = workersURL.ElementAt(workersURL.IndexOfKey(id)).Value;
                getRemoteWorker(workerURL).UnfreezeW();
            }

            else if (splits[0].Equals("FREEZEC"))
            {
                /*FREEZEC <ID>: Disables the communication of the job tracker aspect of a worker node
                in order to simulate its failures.*/

                int id = Int32.Parse(splits[1]);
                String workerURL = workersURL.ElementAt(workersURL.IndexOfKey(id)).Value;
                getRemoteWorker(workerURL).FreezeC();
            }

            else if (splits[0].Equals("UNFREEZEC"))
            {
                /*UNFREEZEC <ID>: Undoes the effects of a previous FREEZEC command.*/

                int id = Int32.Parse(splits[1]);
                String workerURL = workersURL.ElementAt(workersURL.IndexOfKey(id)).Value;
                getRemoteWorker(workerURL).UnfreezeC();
            }
            else return false;
                //gui.Alert("Command not found!");
            return true;
        }

        #region Script Methods
        internal void runScript(IList<String> script)
        {
            foreach (string line in script)
            {
                computeCommand(line);
            }

        }

        internal void runScriptLine(IList<String> script, int lineNumber)
        {
            computeCommand(script[lineNumber]);

        }
        #endregion

        #region Private Methods
        private string parseIP(string url)
        {
            string[] parts = url.Split(new Char[] { '/' });
            string[] parts2 = parts[2].Split(new Char[] { ':' });
            return parts2[0];
        }

        private int parsePort(string url)
        {
            string[] parts = url.Split(new Char[] { '/' });
            string[] parts2 = parts[2].Split(new Char[] { ':' });
            return Int32.Parse(parts2[1]);
        }
        #endregion

        #region Services
        public bool CreateWorker(int id, string workerURL)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("MapWorker.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = id + " " + this.parseIP(workerURL) + " " + this.parsePort(workerURL);

            Process.Start(startInfo);

            return true;
        }

        public bool CreateWorkerEntry(int id, string workerURL, string entryPoint)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("MapWorker.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = id + " " + this.parseIP(workerURL) + " " + this.parsePort(workerURL) + " " + entryPoint;

            Process.Start(startInfo);

            return true;
        }
        #endregion

    }
}
