using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace simpleTcpCommunication
{
  
    public class SimpleTcpServer
    {
        //to control listening process
        internal  ManualResetEvent allDone = new ManualResetEvent(false);
        //variable for main listen thread
        internal IAsyncResult iARlistener;
        // list of client connetions with separate additional data
        internal List<SimpleTcpItem> items;
        // you can set up maximum client count. set to 0 for unlimited
        internal int clientLImit = 0;
        internal Socket listener;
        // to check if we are listening on port
        internal bool listening = false;
        // to start terminating procedure
        internal bool terminate = false;
        //object with main task to execute actions

        /// <summary>
        /// Action to process log messages
        /// </summary>
        public Action<string> OnLog { get; set; }
        /// <summary>
        /// Action to process data received event
        /// </summary>
        public Action<int> OnDataReceived { get;  set; }

        /// <summary>
        /// Returns if server is listening for new connections
        /// </summary>
        /// <returns></returns>
        public bool isListening()
        {
            return listening;
        }



        public  SimpleTcpServer()
        {
            items = new List<SimpleTcpItem>();
        }

        /// <summary>
        /// Start main listening process
        /// </summary>
        /// <param name="listenPort"></param>
        public void StartListening(int listenPort)
        {
            terminate = false;
            if (listener != null && this.listening) return;//brak mozliwosci ponownego wlaczenia
            IPAddress ipAddress = IPAddress.Any;// 
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, listenPort);
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
                log("Listener cleaned");
            }
            listener = new Socket(AddressFamily.InterNetwork,
                  SocketType.Stream, ProtocolType.Tcp);
            Task.Factory.StartNew(() => {
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);
                    log("Starting to listen on port "+localEndPoint.Port.ToString());

                    this.listening = true;
                    while (terminate == false)
                    {
                        allDone.Reset();
                        log("Waiting for a connections...");
                        iARlistener = listener.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listener);
                        allDone.WaitOne();
                    }
                    this.listening = false;
                    log("Listen port closed.");

                }
                catch (Exception e)
                {
                    log(e.ToString());
                }
            });
        }

        internal void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            if (this.terminate) return;
            try
            {
                Socket handler = listener.EndAccept(ar);

                // Create the socket state object.
                SimpleTcpItem state = new SimpleTcpItem();
                items.Add(state);
                state.socket = handler;
                handler.BeginReceive(state.buffer, 0, SimpleTcpItem.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                log("New client connected");
            }
            catch (Exception e)
            {
                log(e.Message);
            }

        }

        /// <summary>
        /// Close everything and release listen process
        /// </summary>
        public void StopListening()
        {
            //set closing bool
            terminate = true;
            //close each client
            for (int i = 0; i < items.Count; i++) SocketClose(i);
            //empty client table
            items.Clear();
            if (! iARlistener.IsCompleted)
            {
                //terminate listening process...
                listener.Close();


            }

        }

        /// <summary>
        /// Return if client connection by given index is active or not
        /// </summary>
        /// <param name="num">client connection index</param>
        /// <returns></returns>
        public bool IsConnected(int num) //this Socket socket)
        {
            try
            {
                return !(items[num].socket.Poll(1, SelectMode.SelectRead) && (items[num].socket.Available) == 0);
            }
            catch (SocketException) { return false; }
        }

        /// <summary>
        /// Send data to client by given index
        /// </summary>
        /// <param name="num">Socket index, when working with many clients</param>
        /// <param name="data">Data to be sent to client</param>
        public void SocketSend(int num, string data)
        {
            //num - numer polaczenia
            if (0 <= num && num <= items.Count)
            {
                if (items[num].socket.Connected)
                    Send(items[num].socket, data);
            }
        }


        internal void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            SimpleTcpItem state = (SimpleTcpItem)ar.AsyncState;
            Socket handler = state.socket;
            if (!handler.Equals(null) && handler.Connected)
                try
                {
                    // Read data from the client socket. 
                    int bytesRead = handler.EndReceive(ar);

                    if (bytesRead > 0 && state.socket != null && state.socket.Connected)
                    {
                        state.received.Add(new Tuple<byte[],int>(state.buffer,bytesRead));

                        int no = 0;
                        //find index of connection I am
                        for (no = 0; no < items.Count; no++)
                        {
                            if (items[no] == state) break;
                        }
                        Action<int> tt = OnDataReceived;
                        tt?.Invoke(no);//? - this means if null, do not run!

                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, SimpleTcpItem.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);

                    }
                }
                catch (Exception e)
                {
                    //a moze zamkniecie ?
                    log(e.ToString());
                }
        }

        /// <summary>
        /// Close connection to selected client by index
        /// </summary>
        /// <param name="num"></param>
        public void SocketClose(int num)
        {
            //num - numer polaczenia
            if (0 <= num && num <= items.Count)
            {
                if (items[num].socket.Connected)
                    items[num].socket.Disconnect(false);
            }
            log("Client socket " + num.ToString() + " closed.");
        }

        internal void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }


        internal void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
        }

        /// <summary>
        /// Get data from a given client index
        /// </summary>
        /// <param name="num"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool getData(int num, ref string data)
        {
            //no client with this id
            if ((num < 0) || (items.Count <= num)) return false;
            if (items[num].received.Count > 0)
            {
                data = Encoding.ASCII.GetString(items[num].received[0].Item1, 0, items[num].received[0].Item2);    //do not know the length...
                items[num].received.RemoveAt(0);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Get data from any client
        /// </summary>
        /// <param name="data"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool getData( ref string data,ref int num)
        {
            //no clients
            if (items.Count ==0) return false;
            for (int i=0; i<items.Count; i++)
            {
                if (items[i].received.Count > 0)
                {
                    num = i;
                    data = Encoding.ASCII.GetString(items[num].received[0].Item1, 0, items[num].received[0].Item2);    //do not know the length...
                    items[num].received.RemoveAt(0);
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Link to external log Event/Action
        /// </summary>
        /// <param name="s"></param>
        internal void log (string s)
        {
            if (OnLog == null) return;
            OnLog(s);
        }


    }


  

}
