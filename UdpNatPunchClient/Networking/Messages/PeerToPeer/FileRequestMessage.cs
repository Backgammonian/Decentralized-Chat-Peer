namespace Networking.Messages
{
    public sealed class FileRequestMessage : BaseMessage
    {
        public FileRequestMessage(string newDownloadID, string fileID, string fileName)
        {
            Type = NetworkMessageType.FileRequest;
            NewDownloadID = newDownloadID;
            FileID = fileID;
            FileName = fileName;
        }

        public string NewDownloadID { get; set; }
        public string FileID { get; set; }
        public string FileName { get; set; }
    }
}
