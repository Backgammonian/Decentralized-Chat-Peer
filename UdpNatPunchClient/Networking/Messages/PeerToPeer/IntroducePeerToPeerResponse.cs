namespace Networking.Messages
{
    public sealed class IntroducePeerToPeerResponse : BaseMessage
    {
        public IntroducePeerToPeerResponse(string id, string nickname, string pictureBase64)
        {
            Type = NetworkMessageType.IntroducePeerToPeerResponse;
            ID = id;
            Nickname = nickname;
            PictureBase64 = pictureBase64;
        }

        public string ID { get; }
        public string Nickname { get; }
        public string PictureBase64 { get; }
    }
}
