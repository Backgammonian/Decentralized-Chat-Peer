namespace Networking.Messages
{
    public class UserInfoFromTracker
    {
        public UserInfoFromTracker(string nickname, string id)
        {
            Nickname = nickname;
            ID = id;
        }

        public string Nickname { get; private set; }
        public string ID { get; private set; }
    }

    public class ListOfUsersWithDesiredNicknameMessage : BaseMessage
    {
        public ListOfUsersWithDesiredNicknameMessage(UserInfoFromTracker[] users)
        {
            Type = NetworkMessageType.ListOfUsersWithDesiredNickname;
            Users = users;
        }

        public UserInfoFromTracker[] Users { get; }
    }
}
