using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class IncomingFileMessageModel : BaseMessageModel
    {
        public IncomingFileMessageModel(FileMessage fileMessage, UserModel server) : base(fileMessage.MessageID)
        {
            AvailableFile = new AvailableFile(fileMessage.SharedFileInfo, server);
        }

        public AvailableFile AvailableFile { get; }
    }
}
