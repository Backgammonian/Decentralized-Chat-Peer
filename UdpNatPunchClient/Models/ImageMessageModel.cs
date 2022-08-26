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

        public ImageItem Image { get; }
    }
}
