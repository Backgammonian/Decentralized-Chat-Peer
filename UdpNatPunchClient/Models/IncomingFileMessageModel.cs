using NetworkingLib.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class IncomingFileMessageModel : BaseMessageModel
    {
        public IncomingFileMessageModel(FileMessage fileMessage, UserModel server) : base(fileMessage.MessageID)
        {
            var sharedFileInfo = new SharedFileInfo(fileMessage.SharedFileHash,
                fileMessage.SharedFileID,
                fileMessage.SharedFileName,
                fileMessage.SharedFileSize,
                fileMessage.SharedFileNumberOfSegments);
            AvailableFile = new AvailableFile(sharedFileInfo, server);
        }

        public AvailableFile AvailableFile { get; }
    }
}
