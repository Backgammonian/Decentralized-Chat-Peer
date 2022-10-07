namespace NetworkingLib.Messages
{
    public sealed class UserNotFoundErrorMessage : BaseMessage
    {
        public UserNotFoundErrorMessage(string userInfo)
        {
            Type = NetworkMessageType.UserNotFoundError;
            UserInfo = userInfo;
        }

        public string UserInfo { get; set; }
    }
}
