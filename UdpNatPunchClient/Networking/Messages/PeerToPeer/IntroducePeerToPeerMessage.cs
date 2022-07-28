namespace Networking.Messages
{
    public sealed class IntroducePeerToPeerMessage : BaseMessage
    {
        public IntroducePeerToPeerMessage(string id)
        {
            Type = NetworkMessageType.IntroducePeerToPeer;
            ID = id;
        }

        public string ID { get; }
    }
}
