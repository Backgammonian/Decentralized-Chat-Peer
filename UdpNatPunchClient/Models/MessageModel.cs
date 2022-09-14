using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class MessageModel : BaseMessageModel
    {
        //outgoing
        public MessageModel(string content) : base()
        {
            Content = content;
        }

        //incoming
        public MessageModel(TextMessageToPeer messageFromOutside) : base(messageFromOutside.MessageID)
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
