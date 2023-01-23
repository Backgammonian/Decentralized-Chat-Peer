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

            FileHash = sharedFile.Hash;
            FileID = sharedFile.ID;
            Name = sharedFile.Name;
            Size = sharedFile.Size;
            NumberOfSegments = sharedFile.NumberOfSegments;
        }

        public SharedFileInfo(string sharedFileHash,
            string sharedFileID,
            string sharedFileName,
            long sharedFileSize,
            long sharedFileNumberOfSegments) 
        {
            FileHash = sharedFileHash; 
            FileID = sharedFileID; 
            Name = sharedFileName;
            Size = sharedFileSize;
            NumberOfSegments = sharedFileNumberOfSegments;
        }

        public string FileHash { get; set; } = string.Empty;
        public string FileID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public long NumberOfSegments { get; set; }
    }
}