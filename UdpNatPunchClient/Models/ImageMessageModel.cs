using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class ImageMessageModel : BaseMessageModel
    {
        //outgoing
        public ImageMessageModel(string authorID, ImageItem image) : base(authorID)
        {
            Image = image;
        }

        //incoming
        public ImageMessageModel(ImageMessageToPeer messageFromOutside, ImageItem incomingImage) : base(messageFromOutside.AuthorID, messageFromOutside.MessageID)
        {
            Image = incomingImage;
        }

        public ImageItem Image { get; }
    }
}
