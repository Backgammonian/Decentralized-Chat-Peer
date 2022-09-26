namespace Networking.Messages
{
    public sealed class DownloadCancellationMessage : BaseMessage
    {
        public DownloadCancellationMessage(string downloadID)
        {
            Type = NetworkMessageType.CancelDownload;
            DownloadID = downloadID;
        }

        public string DownloadID { get; set; }
    }
}
