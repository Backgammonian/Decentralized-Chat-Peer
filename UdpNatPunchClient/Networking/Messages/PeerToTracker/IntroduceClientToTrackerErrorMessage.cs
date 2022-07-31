namespace Networking.Messages
{
    public sealed class IntroduceClientToTrackerErrorMessage : BaseMessage
    {
        public IntroduceClientToTrackerErrorMessage()
        {
            Type = NetworkMessageType.IntroduceClientToTrackerError;
        }
    }
}