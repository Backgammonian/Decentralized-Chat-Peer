namespace NetworkingLib.Messages
{
    public sealed class FileMessage : BaseMessage
    {
        public FileMessage(string sharedFileHash,
            string sharedFileID,
            string sharedFileName,
            long sharedFileSize,
            long sharedFileNumberOfSegments,
            string messageID)
        {
            Type = NetworkMessageType.FileMessage;
            SharedFileHash = sharedFileHash;
            SharedFileID = sharedFileID;
            SharedFileName = sharedFileName;
            SharedFileSize = sharedFileSize;
            SharedFileNumberOfSegments = sharedFileNumberOfSegments;
            MessageID = messageID;
        }

        public string SharedFileHash { get; set; } = string.Empty;
        public string SharedFileID { get; set; } = string.Empty;
        public string SharedFileName { get; set; } = string.Empty;
        public long SharedFileSize { get; set; }
        public long SharedFileNumberOfSegments { get; set; }
        public string MessageID { get; set; }
    }
}
