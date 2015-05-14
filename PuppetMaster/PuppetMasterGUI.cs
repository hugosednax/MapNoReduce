using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterGUI : Form
    {
        PuppetMaster puppetMaster;
        IList<String> script;

        public PuppetMasterGUI(PuppetMaster puppetMaster)
        {
            this.puppetMaster = puppetMaster;
            InitializeComponent();
            listView1.Scrollable = true;
            listView1.View = View.Details;
            listView1.Columns.Add("Script", 600, HorizontalAlignment.Left);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            script = new List<String>();

            openFileDialog1.InitialDirectory = @"c:\\";
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
                            script.Clear();
                            listView1.Items.Clear();
                            string line;
                            StreamReader reader = new StreamReader(myStream);
                            while ((line = reader.ReadLine()) != null)
                            {
                                Console.WriteLine("Input: " + line);
                                script.Add(line);
                                listView1.Items.Add(new ListViewItem(line, 0));
                            }
                            reader.Close();
                            myStream.Close();

                        }
                    }
                }
                catch (Exception)
                {
                    //Log.WriteToLog("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            //puppetMaster.runScript(script);
        }

        private void submitBtn_MouseClick(object sender, MouseEventArgs e)
        {
            puppetMaster.computeCommand(cmdBox.Text);
        }

        public void Alert(string msg)
        {
            MessageBox.Show(msg);
        }

        private void exeNextBtn_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                puppetMaster.computeCommand(item.Text);
            }
        }

        private void exeAllBtn_MouseClick(object sender, MouseEventArgs e)
        {
            puppetMaster.runScript(script);
        }
    }
}
