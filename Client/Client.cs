using PADIMapInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLib
{
    public class Client : MarshalByRefObject, IClient
    {
        #region Class Variables
        IWorker entryProxy;
        string mapperName;
        int numberOfSlicesReceived, numSplits;
        IList<String> input;
        byte[] fileDll;
        string outputFolder;
        IList<int> splitsReceived;
        Object splitLock = new Object();
        string url;
        string jtUrl = "";
        #endregion

        #region Initializations
        public Client(string ip, int port)
        {
            url = "tcp://" + ip + ":" + port + "/C";
            this.mapperName = "Mapper";
            numberOfSlicesReceived = 0;
            numSplits = 0;
            input = new List<String>();
            splitsReceived = new List<int>();
        }

        public override object InitializeLifetimeService()
        {

            return null;

        }

        public void setMapperName(string mapp) {
            mapperName = mapp;
        }

        public void Init(string entryIP, int entryPort)
        {
            entryProxy = (IWorker)Activator.GetObject(typeof(IWorker), "tcp://" + entryIP + ":" + entryPort + "/W");
        }

        public void loadInputFile(Stream inputFile)
        {
            string line;
            input.Clear();
            StreamReader reader = new StreamReader(inputFile);
            while ((line = reader.ReadLine()) != null)
            {
                input.Add(line);
            }
            reader.Close();
            inputFile.Close();
        }

        public void loadDllFile(Stream fileDllStream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                fileDllStream.CopyTo(ms);
                this.fileDll = ms.ToArray();
            }
        }
        #endregion

        #region Services
        public void Submit(int numSlices)
        {
            numSplits = numSlices;
            entryProxy.SubmitJob(numSlices, input.Count(), url);
        }

        public bool HasReceivedSplit(int indexBegin, int indexEnd)
        {
            int splitNumber = ((indexBegin - ((input.Count / numSplits > 0 && indexBegin > 0) ? 1 : 0)) / (input.Count / numSplits)) + 1;
            return splitsReceived.Contains(splitNumber);
        }

        public void outputSplit(IList<KeyValuePair<string, string>> result, int indexBegin, int indexEnd, string jtUrl)
        {
            this.jtUrl = jtUrl;
            if (numSplits != 0)
            {
                int splitNumber = ((indexBegin - ((input.Count / numSplits > 0 && indexBegin > 0) ? 1 : 0)) / (input.Count / numSplits)) + 1;
                if (splitsReceived.Contains(splitNumber)) return;
                lock (splitLock)
                {
                    numberOfSlicesReceived++;
                }
                splitsReceived.Add(splitNumber);
                Console.WriteLine("Just received splitNumber: " + splitNumber);
                FileStream f = new FileStream(outputFolder + "/" + splitNumber + ".out", FileMode.Append, FileAccess.Write);
                StreamWriter s = new StreamWriter(f);
                foreach (KeyValuePair<string, string> keyValues in result)
                {
                    string line = keyValues.Key + " " + keyValues.Value;
                    s.WriteLine(line);
                }
                s.Dispose();
                f.Dispose();
                s.Close();
                f.Close();
            }
            if (numberOfSlicesReceived == numSplits) {
                IWorker jt = (IWorker)Activator.GetObject(typeof(IWorker), jtUrl);
                jt.FinishedJob();
                splitsReceived.Clear();
                try
                {
                    entryProxy.showElapsedTime();
                }
                catch (Exception)
                {
                }
                Environment.Exit(0);
            }

        }

        public IList<string> getSplit(int indexBegin, int indexEnd)
        {
            IList<string> splitList = new List<string>();
            int limit = Util.Clamp(indexEnd, 0, input.Count);
            int begin = Util.Clamp(indexBegin, 0, input.Count);

            for (int i = begin; i <= limit; i++)
            {
                splitList.Add(input[i]);
            }

            return splitList;
        }

        public byte[] getDLLSettings()
        {
            return fileDll;
        }

        public string getMapperName(){
            return mapperName;
        }
        #endregion

        #region Handlers
        public void setOutputFolder(string folderPath)
        {
            this.outputFolder = folderPath;
        }
        #endregion
    }
}
