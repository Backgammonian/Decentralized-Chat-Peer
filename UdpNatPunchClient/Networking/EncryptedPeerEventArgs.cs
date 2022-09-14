using System;

namespace Networking
{
    public sealed class EncryptedPeerEventArgs : EventArgs
    {
        public EncryptedPeerEventArgs(EncryptedPeer peer)
        {
            Peer = peer;
        }

        public EncryptedPeer Peer { get; }
    }
}
