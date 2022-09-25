using Networking;

namespace UdpNatPunchClient.Models
{
    public sealed class SharedFileInfo
    {
        public SharedFileInfo() //user in file client
        {
            Name = string.Empty;
            Hash = string.Empty;
        }

        public SharedFileInfo(SharedFile sharedFile) //used in file server
        {
            Name = sharedFile.Name;
            Size = sharedFile.Size;
            NumberOfSegments = sharedFile.NumberOfSegments;
            Hash = sharedFile.Hash;
            Server = null;
        }

        public string Name { get; set; }
        public long Size { get; set; }
        public long NumberOfSegments { get; set; }
        public string Hash { get; set; }
        public EncryptedPeer? Server { get; set; }
    }
}