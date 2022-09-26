using System;

namespace UdpNatPunchClient.Models
{
    public sealed class SharedFileEventArgs : EventArgs
    {
        public SharedFileEventArgs(SharedFile sharedFile)
        {
            SharedFile = sharedFile;
        }

        public SharedFile SharedFile { get; }
    }
}
