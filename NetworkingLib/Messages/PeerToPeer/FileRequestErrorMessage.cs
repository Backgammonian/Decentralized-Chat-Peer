namespace NetworkingLib.Messages
{
    public sealed class FileRequestErrorMessage : BaseMessage
    {
        public FileRequestErrorMessage(string downloadID, string fileName)
        {
            Type = NetworkMessageType.FileRequestError;
            DownloadID = downloadID;
            FileName = fileName;
        }

        public string DownloadID { get; set; }
        public string FileName { get; set; }
    }
}
