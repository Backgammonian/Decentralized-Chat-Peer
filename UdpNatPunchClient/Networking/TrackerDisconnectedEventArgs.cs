using System;
using System.Net;

namespace Networking
{
    public sealed class TrackerDisconnectedEventArgs : EventArgs
    {
        public TrackerDisconnectedEventArgs(IPEndPoint endPoint)
        {
            TrackerEndPoint = endPoint;
        }

        public IPEndPoint TrackerEndPoint { get; }
    }
}
