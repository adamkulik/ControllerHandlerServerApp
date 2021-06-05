using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ControllerHandlerServerApp
{
    public class ConnectionHandler
    {
        private Socket udpSocket;
        private Socket handler;
        
        public int PortNumber { get; set; }
        public ConnectionHandler(int port)
        {
            new Socket(SocketType.Dgram, ProtocolType.Udp);
            PortNumber = port;
            
        }

        public void StartServer()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PortNumber);
            try
            {
                udpSocket.Bind(localEndPoint);
                udpSocket.BeginAccept(new AsyncCallback(AcceptConnection),udpSocket);
            
            }
            catch()
            {
                throw; // TODO: implement
            }
        }

        private void AcceptConnection(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            handler = listener.EndAccept(ar);
        }

        public void StopServer()
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            udpSocket.Shutdown(SocketShutdown.Both);
            udpSocket.Close();
        }
        public void SendData(byte[] data)
        {
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(EndSend), handler);
        }

        private void EndSend(IAsyncResult ar)
        {
            handler.EndSend(ar);
        }
    }
}