using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class MessageModel : BaseMessageModel
    {
        //outgoing
        public MessageModel(string authorID, string content) : base(authorID)
        {
            Content = content;
        }

        //incoming
        public MessageModel(TextMessageToPeer messageFromOutside) : base(messageFromOutside.AuthorID, messageFromOutside.MessageID)
        {
            Content = messageFromOutside.Content;
        }

        //tracker, incoming and outgoing
        public MessageModel(string input, MessageDirection direction) : base(direction)
        {
            Content = input;
        }

        public string Content { get; }
    }
}
