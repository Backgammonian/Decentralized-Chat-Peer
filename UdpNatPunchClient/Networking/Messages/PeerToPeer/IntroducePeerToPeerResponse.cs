namespace Networking.Messages
{
    public sealed class IntroducePeerToPeerResponse : BaseMessage
    {
        public IntroducePeerToPeerResponse(string id, string nickname, byte[] pictureByteArray, string pictureExtension)
        {
            Type = NetworkMessageType.IntroducePeerToPeerResponse;
            ID = id;
            Nickname = nickname;
            PictureByteArray = pictureByteArray;
            PictureExtension = pictureExtension;
        }

        public string ID { get; }
        public string Nickname { get; }
        public byte[] PictureByteArray { get; }
        public string PictureExtension { get; }
    }
}
