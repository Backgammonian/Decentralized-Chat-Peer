using UdpNatPunchClient.Models;

namespace Networking.Messages
{
    public sealed class FileMessage : BaseMessage
    {
        public FileMessage(SharedFileInfo sharedFileInfo, string messageID)
        {
            Type = NetworkMessageType.FileMessage;
            SharedFileInfo = sharedFileInfo;
            MessageID = messageID;
        }

        public SharedFileInfo SharedFileInfo { get; set; }
        public string MessageID { get; set; }
    }
}
