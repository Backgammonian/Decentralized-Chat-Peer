using System.Net;
using System.Net.Sockets;

namespace Helpers
{
    public class LocalAddressResolver
    {
        public IPAddress? GetLocalAddress()
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;

            return endPoint?.Address;
        }
    }
}
