using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClientLib;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using PADIMapInterfaces;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UserLevelApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Log.WriteToLog(args.Length + "");

            if (args.Length > 0)
            {
                Console.WriteLine("Client Console\r\nPress any key to close...");
                //SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> <DLL>:
                

                //this.parseIP(entryURL) + " " + this.parsePort(entryURL) + " " + outputPath + " " + filePath + " " + dllPath + " " + className + clientPort
                string entryIP = args[0];
                int entryPort = Int32.Parse(args[1]);
                string outputPath = args[2];
                string filePath = args[3];
                string dllPath = args[4];
                string className = args[5];
                int slices = Int32.Parse(args[6]);
                int port = Int32.Parse(args[7]);
                string internetIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();

                Client client = new Client(internetIp, port);
                TcpChannel channel = new TcpChannel(port);
                RemotingServices.Marshal(client, "C", typeof(IClient));

                client.Init(entryIP, entryPort);

                client.setOutputFolder(outputPath);

                Stream myStream = File.OpenRead(filePath);
                client.loadInputFile(myStream);

                Stream dllStream = File.OpenRead(dllPath);
                client.loadDllFile(dllStream);
                client.setMapperName(className);

                client.Submit(slices);

                /*if (Log.Debug)
                {
                    Log.WriteToDebug("Client was called with entryIP=" + entryIP + " filePath=" + filePath + " outputPath=" + outputPath + " slices=" + slices + " className=" +
                        className + " dllPath=" + dllPath);
                }*/

                Console.ReadLine();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ClientGUI());
            }
            
        }
    }
}
