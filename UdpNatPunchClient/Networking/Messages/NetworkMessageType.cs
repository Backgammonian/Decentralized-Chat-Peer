namespace Networking.Messages
{
    public enum NetworkMessageType : byte
    {
        Empty = 0,
        ExampleMessage = 10,

        IntroduceClientToTracker,
        IntroduceClientToTrackerResponse,
        IntroduceClientToTrackerError,
        CommandToTracker,
        CommandReceiptNotification,
        CommandToTrackerError,
        UserConnectionResponse,
        ForwardedConnectionRequest,
        UserNotFoundError,
        PingResponse,
        TimeResponse,
        UpdatedInfoForTracker,

        IntroducePeerToPeer,
        IntroducePeerToPeerResponse,
        TextMessage,
        MessageReceiptNotification,
        MessageReadNotification,
        UpdatedInfoForPeer,
        UpdatedProfilePictureForPeer,
    }
}