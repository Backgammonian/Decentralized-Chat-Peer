namespace Networking.Messages
{
    public sealed class FileRequestErrorMessage : BaseMessage
    {
        public FileRequestErrorMessage(string fileHash)
        {
            Type = NetworkMessageType.FileRequestError;
            FileHash = fileHash;
        }

        public string FileHash { get; set; }
    }
}
