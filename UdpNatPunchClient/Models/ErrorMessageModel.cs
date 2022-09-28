namespace UdpNatPunchClient.Models
{
    public sealed class ErrorMessageModel : BaseMessageModel
    {
        public ErrorMessageModel(string text) : base(MessageDirection.Incoming)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
