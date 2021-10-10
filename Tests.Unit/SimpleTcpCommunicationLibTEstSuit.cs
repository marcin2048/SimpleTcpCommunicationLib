using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;


namespace Tests.Unit
{
    public class SimpleTcpCommunicationLibTEstSuit
    {


        [Fact]
        public void should_make_one_tcp_connection_and_send_data()
        {
            //Arrange
            var srv1 = new  simpleTcpCommunication.SimpleTcpServer();
            var cnt1 = new simpleTcpCommunication.SimpleTcpClient();
            string datatosend = "just smimple data";
            string datareceived = "";
            int socketno = 0;
            bool received = false;



            //Act
            srv1.StartListening(8080);
            Thread.Sleep(200);
            cnt1.Connect("127.0.0.1", 8080);
            //send data
            cnt1.SendData(datatosend);
            Thread.Sleep(200);
            //try to receive data
            received = srv1.getData(ref datareceived, ref socketno);//
            //close connections
            srv1.SocketClose(0);
            srv1.StopListening();

            //Assert
            Assert.Equal(datatosend, datareceived);


        }
    }
}
