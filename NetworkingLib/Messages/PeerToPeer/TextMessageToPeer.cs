namespace NetworkingLib.Messages
{
    public sealed class TextMessageToPeer : BaseMessage
    {
        public TextMessageToPeer(string messageID, string content)
        {
            Type = NetworkMessageType.TextMessage;
            MessageID = messageID;
            Content = content;
        }

        public string MessageID { get; set; }
        public string Content { get; set; }
    }
}
