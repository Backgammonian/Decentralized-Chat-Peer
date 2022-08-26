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

            var newPicture = ImageItem.TrySaveByteArrayAsImage(pictureByteArray, pictureExtension);
            if (newPicture != null)
            {
                Picture = newPicture;
                if (Picture.TryLoadImage())
                {
                    return true;
                }
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
    }
}
