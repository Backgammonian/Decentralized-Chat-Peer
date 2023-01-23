namespace NetworkingLib.Messages
{
    public sealed class IntroduceClientToTrackerResponseMessage : BaseMessage
    {
        public IntroduceClientToTrackerResponseMessage(string endPointString)
        {
            Type = NetworkMessageType.IntroduceClientToTrackerResponse;
            EndPointString = endPointString;
        }

        public string EndPointString { get; set; }
    }
}
