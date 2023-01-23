using System;
using NetworkingLib.Messages;

namespace NetworkingLib
{
    public sealed class NetEventArgs : EventArgs
    {
        public NetEventArgs(EncryptedPeer encryptedPeer, NetworkMessageType type, string json)
        {
            EncryptedPeer = encryptedPeer;
            Type = type;
            Json = json;
        }

        public EncryptedPeer EncryptedPeer { get; }
        public NetworkMessageType Type { get; }
        public string Json { get; }
    }
}
