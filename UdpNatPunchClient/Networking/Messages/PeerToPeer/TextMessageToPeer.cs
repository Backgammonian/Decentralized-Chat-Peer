namespace Networking.Messages
{
    public sealed class TextMessageToPeer : BaseMessage
    {
        public TextMessageToPeer(string messageID, string content, string authorID)
        {
            Type = NetworkMessageType.TextMessage;
            MessageID = messageID;
            Content = content;
            AuthorID = authorID;
        }

        public string MessageID { get; set; }
        public string Content { get; set; }
        public string AuthorID { get; set; }
    }
}
