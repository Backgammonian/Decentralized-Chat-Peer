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
        public MessageModel(TextMessageToPeer messageFromOutside) : base(messageFromOutside)
        {
            Content = messageFromOutside.Content;
        }

        //tracker, incoming and out
        public MessageModel(string input, MessageDirection direction) : base(direction)
        {
            Content = input;
        }

        public string Content { get; }
    }
}
