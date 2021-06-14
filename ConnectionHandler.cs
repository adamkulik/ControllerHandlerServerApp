using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ControllerHandlerServerApp
{
    public class ConnectionHandler
    {
        private Socket udpSocket;
        private Socket handler;
        private IPAddress address;
        private bool connected = false;

        public int PortNumber { get; set; }
        public ConnectionHandler(string ipAddress, int port)
        {
            udpSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            address = new IPAddress(ipAddress.Split('.').Select(x => Byte.Parse(x)).ToArray());
            PortNumber = port;

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
            udpSocket.SendTo(data, new IPEndPoint(address, PortNumber));
        }

    }
}