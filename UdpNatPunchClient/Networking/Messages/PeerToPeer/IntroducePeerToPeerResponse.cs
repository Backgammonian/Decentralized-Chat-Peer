namespace Networking.Messages
{
    public sealed class IntroducePeerToPeerResponse : BaseMessage
    {
        public IntroducePeerToPeerResponse(string id)
        {
            Type = NetworkMessageType.IntroducePeerToPeerResponse;
            ID = id;
        }

        public string ID { get; }
    }
}
