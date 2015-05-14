using PADIMapInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            int port = 20001;

            if(args.Length > 0)
                port = Int32.Parse(args[0]);

            PuppetMaster puppetMaster = new PuppetMaster();
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(puppetMaster, "PM", typeof(IPuppetMaster));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PuppetMasterGUI(puppetMaster));
        }
    }
}
