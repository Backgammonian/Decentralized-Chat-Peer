namespace UdpNatPunchClient.Models
{
    public sealed class OutgoingFileMessageModel : BaseMessageModel
    {
        public OutgoingFileMessageModel(SharedFileInfo fileInfo) : base()
        {
            FileInfo = fileInfo;
        }

        public SharedFileInfo FileInfo { get; }
    }
}
