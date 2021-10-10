using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace simpleTcpCommunication
{
    public class SimpleTcpClient  : SimpleTcpItem
    {
        internal TcpClient tcp1;
        internal Stream stm;

        /// <summary>
        /// Action to process data received event
        /// </summary>
        public Action<string> OnDataReceived { get;  set; }
        public Action<string> OnLog { get; set; }

        public SimpleTcpClient()
        {

        }

        /// <summary>
        /// Method to connect to TCP Server
        /// </summary>
        /// <param name="hostname">Server hostname/address</param>
        /// <param name="port">TCP Server port</param>
        public void Connect(string hostname, int port) {
            if (tcp1 == null)
            {
                try
                {
                    tcp1 = new TcpClient();
                    log("Connecting...");
                    tcp1.Connect(hostname, port);
                    ///
                    stm = tcp1.GetStream();
                    log("Connected!");
                    clientReadDataAsync();
                }
                catch (Exception ex)
                {
                    log(ex.Message);
                    tcp1 = null;
                }

            }
        }

        /// <summary>
        /// Data received 
        /// </summary>
        /// <returns></returns>
        private async Task clientReadDataAsync()
        {
            try
            {
                byte[] bytescnt = new byte[2048];
                int i = await stm.ReadAsync(bytescnt, 0, bytescnt.Length);
                if (i > 0)
                {
                    log("Client data IN:" + i.ToString());
                    string dataIn = System.Text.Encoding.UTF8.GetString(bytescnt, 0, i);    //sumujemy to co przyszlo

                    Action<string> tt = OnDataReceived;
                    tt?.Invoke(dataIn);
                }
                if (!tcp1.Connected) return;

                //przerwanie ciaglego oczekiwania na dane - jesli polaczenie nie jest juz aktywne
                bool a1 = tcp1.Client.Poll(1, SelectMode.SelectRead);
                bool a2 = (tcp1.Client.Available == 0);
                if (a1 && a2)
                {
                    return;
                }

                clientReadDataAsync();
            }
            catch (Exception ex)
            {

            }
        }


 

        public void Close()
        {
            if (tcp1 != null)
            {
                stm.Close();
                stm = null;
                tcp1.Close();
                tcp1 = null;
                log("Cleared and closed.");
            }
        }

        /// <summary>
        /// Funkcja wysyłania danych poprzez połączenie Klient do SiteServer
        /// </summary>
        /// <param name="text"></param>
        public void SendData(string text)
        {
            if (tcp1 == null) return;
            if (tcp1 != null)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                stm.Write(bytes, 0, bytes.Length);
            }
        }


        /// <summary>
        /// Pass log to OnLog event/action. Logging is optional.
        /// </summary>
        /// <param name="s"></param>
        internal void log(string s)
        {
            if (OnLog == null) return;
            OnLog(s);
        }

        /// <summary>
        /// Return connection status
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
            if (tcp1 == null) return false;
            //przerwanie ciaglego oczekiwania na dane - jesli polaczenie nie jest juz aktywne
            bool a1 = tcp1.Client.Poll(1, SelectMode.SelectRead);
            bool a2 = (tcp1.Client.Available == 0);
            if (a1 && a2)
            {
                return false;
            }
            return true;
        }
    }
}
