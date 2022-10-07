namespace NetworkingLib.Messages
{
    public sealed class FileSegmentAckMessage : BaseMessage
    {
        public FileSegmentAckMessage(string downloadID)
        {
            Type = NetworkMessageType.FileSegmentAck;
            DownloadID = downloadID;
        }

        public string DownloadID { get; set; }
    }
}
