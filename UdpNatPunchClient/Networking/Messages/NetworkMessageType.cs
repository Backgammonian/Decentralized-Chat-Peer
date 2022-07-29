namespace Networking.Messages
{
    public enum NetworkMessageType : byte
    {
        Empty = 0,
        ExampleMessage = 10,

        IntroduceClientToTracker,
        UserConnectionRequest,
        UserConnectionResponse,
        ForwardedConnectionRequest,

        IntroducePeerToPeer,
        IntroducePeerToPeerResponse,

        TextMessage,
        MessageReceiptNotification,
        MessageReadNotification,
    }
}
