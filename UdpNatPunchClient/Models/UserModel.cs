using System.Threading.Tasks;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class UserModel : PeerModel
    {
        private string _nickname = string.Empty;
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

        public bool TrySetUpdatedPicture(byte[] pictureByteArray, string pictureExtension)
        {
            if (pictureByteArray.Length == 0 ||
                pictureExtension.Length == 0)
            {
                return false;
            }

            var newPicture = ImageItem.TrySaveByteArrayAsImage(
                pictureByteArray,
                pictureExtension,
                Constants.ProfilePictureThumbnailSize.Item1,
                Constants.ProfilePictureThumbnailSize.Item2);

            if (newPicture != null &&
                newPicture.TryLoadImage())
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
            Send(updateMessage);
        }

        public async Task<bool> TrySendImageMessage(ImageMessageModel message)
        {
            var result = await message.Image.TryGetPictureBytes();
            if (result.Item1)
            {
                Messages.Add(message);
                _undeliveredMessages.Add(message);
                _unreadMessages.Add(message);

                var messageToPeer = new ImageMessageToPeer(message.AuthorID,
                    message.MessageID,
                    result.Item2,
                    message.Image.FileExtension.ToLower());
                Send(messageToPeer);

                return true;
            }

            return false;
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
