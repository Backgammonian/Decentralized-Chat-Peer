namespace NetworkingLib.Messages
{
    public sealed class UploadCancellationMessage : BaseMessage
    {
        public UploadCancellationMessage(string uploadID)
        {
            Type = NetworkMessageType.CancelUpload;
            UploadID = uploadID;
        }

        public string UploadID { get; set; }
    }
}
