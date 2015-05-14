using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapInterfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace MapWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            //RemotingConfiguration.Configure("App.config", true);
            if (args.Length == 0)
            {
                Console.WriteLine("Insert ID of the worker (0 don't request entry point): ");
                int workerID = Int32.Parse(Console.ReadLine());
                Worker worker;

                // Init the initial worker (aka known port worker)
                int port = 30001 + workerID;
                TcpChannel channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, false);
                if (workerID == 0)
                {
                    worker = new Worker(workerID, "localhost", port);
                    RemotingServices.Marshal(worker, "W", typeof(IWorker));
                    Console.WriteLine("Worker is listening");
                }
                else
                {
                    /*Console.WriteLine("Insert IP of the worker entry point: ");
                    string entryIP = Console.ReadLine();
                    Console.WriteLine("Insert Port of the worker entry point: ");
                    string entryPort = Console.ReadLine();*/
                    //worker = new Worker(workerID, "localhost", port, "tcp://" + entryIP + ":" + entryPort + "/WorkerProxy");
                    worker = new Worker(workerID, "localhost", port, "tcp://localhost:30001/W");
                    RemotingServices.Marshal(worker, "W", typeof(IWorker));
                    Console.WriteLine("Worker is listening");
                }

                Console.WriteLine("Press Enter to terminate Worker at anytime...    ");
                Console.ReadLine();
            }
            else
            {
                // MapWorker.exe id ip port (entry point url)
                int id = Int32.Parse(args[0]);
                string ip = args[1];
                int port = Int32.Parse(args[2]);

                TcpChannel channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, false);
                Worker worker;

                if (args.Length == 4)
                {
                    string entrypointURL = args[3];
                    worker = new Worker(id, ip, port, entrypointURL);
                } else
                    worker = new Worker(id, ip, port);
                RemotingServices.Marshal(worker, "W", typeof(IWorker));
                Console.WriteLine("Worker is listening");
                Console.ReadLine();
            }
        }

        /*private static void PrintWorkersNetList(IList<Worker> workersList, Worker worker)
        {
            foreach (Worker workr in workersList)
            {
                string result = "\r\n";
                foreach (string ip in workr.getWorkersURL())
                {
                    result += "\t" + ip + "\r\n";
                }

                //Console.WriteLine("Worker " + workr.getUrl() + " got list " + result);
            }
        }*/
    }
}
