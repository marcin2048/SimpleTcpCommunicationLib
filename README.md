# SimpleTcpCommunicationLib
Fast and easy way to manage many TCP connections. 


# Usage

```csharp
        //server variable definition
        internal SimpleTcpServer srv1;

        //server object inicjalization
        srv1 = new SimpleTcpServer();
        srv1.OnDataReceived += OnDataReceived;  //inside SimpleTcpServer task

        //start listening
        int portNo = 8080;
        srv1.StartListening(portNo);


        /// <summary>
        /// Method to maintain data received events. 
        /// Remember this is in socket thread, not form thread! 
        /// If you want to view data on Form, you need to run Invoke(Action ...)
        /// </summary>
        /// <param name="socketnum"></param>
        private void OnDataReceived(int socketnum)
        {
            //received data event
            string data = "";
            while (srv1.getData(socketnum, ref data))
            {
                logCheckThread(data);
            }
        }



```
