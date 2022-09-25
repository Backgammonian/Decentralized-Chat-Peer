namespace Networking.Messages
{
    public sealed class FileSegmentMessage : BaseMessage
    {
        public FileSegmentMessage(string uploadID, byte[] segment)
        {
            Type = NetworkMessageType.FileSegment;
            UploadID = uploadID;
            Segment = segment;
        }

        public string UploadID { get; }
        public byte[] Segment { get; }
    }
}