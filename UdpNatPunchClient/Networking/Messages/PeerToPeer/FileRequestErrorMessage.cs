namespace Networking.Messages
{
    public sealed class FileRequestErrorMessage : BaseMessage
    {
        public FileRequestErrorMessage(string fileName)
        {
            Type = NetworkMessageType.FileRequestError;
            FileName = fileName;
        }

        public string FileName { get; set; }
    }
}
