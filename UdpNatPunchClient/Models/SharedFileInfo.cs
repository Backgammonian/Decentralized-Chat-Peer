using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace UdpNatPunchClient.Models
{
    public sealed class SharedFileInfo : ObservableObject
    {
        public SharedFileInfo(SharedFile? sharedFile)
        {
            if (sharedFile == null)
            {
                return;
            }

            Hash = sharedFile.Hash;
            Name = sharedFile.Name;
            Size = sharedFile.Size;
            NumberOfSegments = sharedFile.NumberOfSegments;
        }

        public string Hash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public long NumberOfSegments { get; set; }
    }
}