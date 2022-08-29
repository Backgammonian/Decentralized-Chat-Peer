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

        public string ID { get; set; }
        public string Nickname { get; set; }
        public byte[] PictureByteArray { get; set; }
        public string PictureExtension { get; set; }
    }
}
