using System.Collections.Generic;

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
        public ListOfUsersWithDesiredNicknameMessage(List<UserInfoFromTracker> users, string nicknameQuery)
        {
            Type = NetworkMessageType.ListOfUsersWithDesiredNickname;
            Users = users;
            NicknameQuery = nicknameQuery;
        }

        public List<UserInfoFromTracker> Users { get; set; }
        public string NicknameQuery { get; set; }
    }
}
