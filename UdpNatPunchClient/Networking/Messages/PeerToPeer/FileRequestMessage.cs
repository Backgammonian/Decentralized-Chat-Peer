namespace Networking.Messages
{
    public sealed class FileRequestMessage : BaseMessage
    {
        public FileRequestMessage(string id, string fileHash)
        {
            Type = NetworkMessageType.FileRequest;
            ID = id;
            FileHash = fileHash;
        }

        public string ID { get; set; }
        public string FileHash { get; set; }
    }
}
