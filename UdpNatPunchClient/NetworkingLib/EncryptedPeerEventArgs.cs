using System;
using System.Net;

namespace NetworkingLib
{
    public sealed class EncryptedPeerEventArgs : EventArgs
    {
        public EncryptedPeerEventArgs(EncryptedPeer peer)
        {
            PeerID = peer.Id;
            EndPoint = peer.EndPoint;
        }

        public int PeerID { get; }
        public IPEndPoint EndPoint { get; }
    }
}
