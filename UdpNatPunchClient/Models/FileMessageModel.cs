using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class FileMessageModel : BaseMessageModel
    {
        private bool _isUpdated;
        private SharedFileInfo? _sharedFileInfo;

        //outgoing
        public FileMessageModel() : base()
        {
            _isUpdated = false;
        }

        //incoming
        public FileMessageModel(FileMessage fileMessage) : base(fileMessage.MessageID)
        {
            _isUpdated = false;
        }

        public SharedFileInfo? FileInfo
        {
            get => _sharedFileInfo;
            private set => SetProperty(ref _sharedFileInfo, value);
        }

        public void UpdateFileInfo(SharedFileInfo fileInfo)
        {
            if (_isUpdated)
            {
                return;
            }

            FileInfo = fileInfo;
            _isUpdated = true;
        }
    }
}
