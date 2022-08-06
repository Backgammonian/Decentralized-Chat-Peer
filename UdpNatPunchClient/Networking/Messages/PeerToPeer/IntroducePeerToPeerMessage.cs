namespace Networking.Messages
{
    public sealed class IntroducePeerToPeerMessage : BaseMessage
    {
        public IntroducePeerToPeerMessage(string id, string nickname, string pictureBase64)
        {
            Type = NetworkMessageType.IntroducePeerToPeer;
            ID = id;
            Nickname = nickname;
            PictureBase64 = pictureBase64;
        }

        public string ID { get; }
        public string Nickname { get; }
        public string PictureBase64 { get; }
    }
}
