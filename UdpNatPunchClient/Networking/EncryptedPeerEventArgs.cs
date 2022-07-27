using System;

namespace Networking
{
    public class EncryptedPeerEventArgs : EventArgs
    {
        public EncryptedPeerEventArgs(EncryptedPeer peer)
        {
            Peer = peer;
        }

        public EncryptedPeer Peer { get; }
    }
}
