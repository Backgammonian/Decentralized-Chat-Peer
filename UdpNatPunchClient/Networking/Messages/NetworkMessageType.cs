namespace Networking.Messages
{
    public enum NetworkMessageType : byte
    {
        Empty = 0,
        ExampleMessage = 10,
        KeepAlive,

        IntroduceClientToTracker,
        IntroduceClientToTrackerResponse,
        IntroduceClientToTrackerError,
        CommandToTracker,
        CommandReceiptNotification,
        CommandToTrackerError,
        UserConnectionResponse,
        ForwardedConnectionRequest,
        ListOfUsersWithDesiredNickname,
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
        ImageIntroduceMessage,
        UpdateImageMessage,
        ImageSendingFailed,
    }
}