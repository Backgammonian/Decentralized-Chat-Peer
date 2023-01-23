using System;
using System.Net;

namespace NetworkingLib
{
    public sealed class ServerDisconnectedEventArgs : EventArgs
    {
        public ServerDisconnectedEventArgs(IPEndPoint endPoint)
        {
            ServerEndPoint = endPoint;
        }

        public IPEndPoint ServerEndPoint { get; }
    }
}
