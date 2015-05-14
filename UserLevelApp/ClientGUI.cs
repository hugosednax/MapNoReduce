using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using PADIMapInterfaces;
using ClientLib;

namespace UserLevelApp
{
    public partial class ClientGUI : Form
    {
        Client client;

        public ClientGUI()
        {
            InitializeComponent();

            int port = 10001;
            client = new Client("localhost", port);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(client, "C", typeof(IClient));
        }

        public void PopUp(string message)
        {
            MessageBox.Show(message);
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            client.loadInputFile(myStream);
                        }
                    }
                }
                catch (Exception)
                {
                    //Log.WriteToLog("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void startJobBt_MouseClick(object sender, MouseEventArgs e)
        {
            client.Init(entryIP.Text, Int32.Parse(entryPort.Text));
            client.Submit(Int32.Parse(numSlicesBox.Text));
        }

        private void mapDllBtn_MouseClick(object sender, MouseEventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "dll files (*.dll)|*.dll|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            client.loadDllFile(myStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void ouputBtn_MouseClick(object sender, MouseEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            client.setOutputFolder(fbd.SelectedPath);
        }
    }
}
