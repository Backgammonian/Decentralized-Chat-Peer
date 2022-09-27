using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace UdpNatPunchClient.Models
{
    public sealed class AvailableFile : ObservableObject
    {
        private bool _isAvailable;

        public AvailableFile(SharedFileInfo sharedFile, UserModel server)
        {
            FileIDFromServer = sharedFile.FileID;
            Hash = sharedFile.FileHash;
            Name = sharedFile.Name;
            Size = sharedFile.Size;
            NumberOfSegments = sharedFile.NumberOfSegments;
            Server = server;
            IsAvailable = true;
        }

        public string FileIDFromServer { get; }
        public string Hash { get; }
        public string Name { get; }
        public long Size { get; }
        public long NumberOfSegments { get; }
        public UserModel Server { get; }

        public bool IsAvailable
        {
            get => _isAvailable;
            private set => SetProperty(ref _isAvailable, value);
        }

        public void MarkAsUnavailable()
        {
            IsAvailable = false;
        }
    }
}