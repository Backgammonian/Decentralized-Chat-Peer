namespace Networking.Messages
{
    public sealed class FileIsNotAvailableMessage : BaseMessage
    {
        public FileIsNotAvailableMessage(string fileID)
        {
            Type = NetworkMessageType.FileIsNotAvailable;
            FileID = fileID;
        }

        public string FileID { get; set; }
    }
}
