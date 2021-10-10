using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Author:     Marcin Kaminski
/// Date:       2021-10-10
/// 
/// Source:     https://github.com/marcin2048
/// </summary>
namespace simpleTcpCommunication
{
    /// <summary>
    /// Item of TCP connection that is used by server (SimpleTcpServer) and client (SimpleTcpClient) classes
    /// </summary>
    public class SimpleTcpItem
    {

        /// <summary>
        /// Socket of the connection
        /// </summary>
        public Socket socket = null;
        /// <summary>
        /// Default buffer size to receive data
        /// </summary>
        public const int BufferSize = 1024;
        /// <summary>
        /// Buffer to receive data
        /// </summary>
        public byte[] buffer = new byte[BufferSize];
        /// <summary>
        /// List of Tuple buffer,length when receiving data 
        /// </summary>
        public List<Tuple<byte[],int>> received;


        public  SimpleTcpItem()
        {
            //initiate list of received data fragments
            received = new List<Tuple<byte[], int>>();
        }

    }
}
