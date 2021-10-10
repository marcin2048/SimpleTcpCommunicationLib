using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace simpleTcpCommunication
{
    public partial class Form1 : Form
    {
        internal SimpleTcpServer srv1;
        internal SimpleTcpClient cnt1;
        public Form1()
        {
            InitializeComponent();

            //server component
            srv1 = new SimpleTcpServer();
            srv1.OnDataReceived += OnDataReceived;  //inside SimpleTcpServer task
            srv1.OnLog += logCheckThread;           //inside SimpleTcpServer task

            //client component
            cnt1 = new SimpleTcpClient();
            cnt1.OnLog += logCheckThread;
            cnt1.OnDataReceived += OnDataREceivedClient;

        }

        private void OnDataREceivedClient(string data)
        {
            logCheckThread("Client data :" + data);
        }

        /// <summary>
        /// Thread safe log function invoke
        /// </summary>
        /// <param name="s">Message to be logged</param>
        private void logCheckThread(string s)
        {
            if (richTextBox1.InvokeRequired)
            {
                Action logaction = delegate { log(s); };
                richTextBox1.Invoke(logaction);
            }
            else
            {
                log(s);
            }
        }
        

        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (srv1.isListening())
            {
                srv1.StopListening();
            } else
            {
                int portNo = 8080;//default
                //portNo = int.Parse(textBox1.Text)
                try
                {
                    portNo = int.Parse(textBox1.Text);
                }
                catch (Exception ex1)
                {
                    log(ex1.Message);
                }
                finally
                {
                    portNo = 8080;//default when parse fails
                }
                srv1.StartListening(portNo);
            }

        }


        /// <summary>
        /// Method to maintain data received events. 
        /// Remember this is in socket thread, not form thread!
        /// </summary>
        /// <param name="socketnum"></param>
        private void OnDataReceived(int socketnum)
        {
            //received data event
            logCheckThread("Data received event! Client ID:"+socketnum.ToString());
            string data = "";
            while (srv1.getData(socketnum, ref data))
            {
                logCheckThread(data);
            }
        }


        /// <summary>
        /// Simple logging method - show messages in richTextBox
        /// </summary>
        /// <param name="s"></param>
        private void log(string s)
        {
            if (richTextBox1.Lines.Count() > 200) richTextBox1.Clear();
            if (richTextBox1.Text.Length > 0) richTextBox1.Text += "\n";
            richTextBox1.Text += s;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cnt1.Connected())
            {
                cnt1.Close();
            }else
            {
                cnt1.Connect(textBox3.Text, int.Parse(textBox2.Text));

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = ! timer1.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //on work:
            srv1.SocketSend(0, "SERVER TO CLIENT");
            cnt1.SendData("CLIENT TO SERVER");
        }

    }
}
