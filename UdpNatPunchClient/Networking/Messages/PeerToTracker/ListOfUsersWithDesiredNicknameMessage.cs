namespace Networking.Messages
{
    public sealed class UserInfoFromTracker
    {
        public UserInfoFromTracker(string nickname, string id)
        {
            Nickname = nickname;
            ID = id;
        }

        public string Nickname { get; set; }
        public string ID { get; set; }
    }

    public sealed class ListOfUsersWithDesiredNicknameMessage : BaseMessage
    {
        public ListOfUsersWithDesiredNicknameMessage(UserInfoFromTracker[] users)
        {
            Type = NetworkMessageType.ListOfUsersWithDesiredNickname;
            Users = users;
        }

        public UserInfoFromTracker[] Users { get; set; }
    }
}
