using System.Threading.Tasks;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class UserModel : PeerModel
    {
        private string _nickname = "Default nickname";
        private ImageItem? _picture;

        public UserModel(EncryptedPeer peer, string id, string nickname) : base(peer)
        {
            ID = id;
            Nickname = nickname;
        }

        public string ID { get; }

        public string Nickname
        {
            get => _nickname;
            private set => SetProperty(ref _nickname, value);
        }

        public ImageItem? Picture
        {
            get => _picture;
            private set => SetProperty(ref _picture, value);
        }

        public void SetUpdatedPersonalInfo(string newNickname)
        {
            Nickname = newNickname;
        }

        public async Task<bool> TrySetUpdatedPicture(byte[]? pictureByteArray, string pictureExtension)
        {
            if (pictureByteArray == null ||
                pictureByteArray.Length == 0 ||
                pictureExtension.Length == 0)
            {
                return false;
            }

            var newPicture = await ImageItem.SaveByteArrayAsImage(pictureByteArray,
                pictureExtension,
                Constants.ProfilePictureThumbnailSize.Item1,
                Constants.ProfilePictureThumbnailSize.Item2);

            if (newPicture != null &&
                await newPicture.TryLoadImage())
            {
                Picture = newPicture;

                return true;
            }

            return false;
        }

        public void SendUpdatedPersonalInfo(string updatedNickname)
        {
            var updateMessage = new UpdatedInfoForPeerMessage(updatedNickname);
            Send(updateMessage);
        }

        public void SendUpdatedProfilePicture(byte[] pictureByteArray, string pictureExtension)
        {
            var updateMessage = new UpdatedProfilePictureForPeerMessage(pictureByteArray, pictureExtension);
            Send(updateMessage, 1);
        }

        public async Task<bool> TrySendImageMessage(string authorID, ImageItem image)
        {
            var bytes = await image.GetPictureBytes();
            if (bytes == null)
            {
                return false;
            }

            var message = new ImageMessageModel(authorID, image);
            _undeliveredMessages.Add(message);
            _unreadMessages.Add(message);
            Messages.Add(message);

            var messageToPeer = new ImageMessageToPeer(message.AuthorID,
                message.MessageID,
                bytes,
                message.Image.FileExtension);

            Send(messageToPeer, 1);

            return true;
        }

        public ImageMessageModel? AddIncomingMessage(ImageMessageToPeer imageMessageFromPeer, ImageItem image)
        {
            var message = new ImageMessageModel(imageMessageFromPeer, image);
            Messages.Add(message);
            _incomingMessages.Add(message);

            HasNewMessages = true;

            return message;
        }
    }
}
