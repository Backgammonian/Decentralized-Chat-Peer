namespace Networking.Messages
{
    public sealed class IntroducePeerToPeerMessage : BaseMessage
    {
        public IntroducePeerToPeerMessage(string id, string nickname, byte[] pictureByteArray, string pictureExtension)
        {
            Type = NetworkMessageType.IntroducePeerToPeer;
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
