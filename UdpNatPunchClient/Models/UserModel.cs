using System;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class UserModel : PeerModel
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

        public async Task TrySetUpdatedPicture(byte[]? pictureByteArray, string pictureExtension)
        {
            if (pictureByteArray == null ||
                pictureByteArray.Length == 0 ||
                pictureExtension.Length == 0)
            {
                return;
            }

            var newPicture = await ImageItem.SaveByteArrayAsImage(pictureByteArray,
                pictureExtension,
                Constants.ProfilePictureThumbnailSize.Item1,
                Constants.ProfilePictureThumbnailSize.Item2);

            if (newPicture != null &&
                await newPicture.TryLoadImage())
            {
                Picture = newPicture;
            }
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

        public ImageMessageModel SendImageIntroductionMessage()
        {
            var message = new ImageMessageModel();
            _undeliveredMessages.Add(message);
            _unreadMessages.Add(message);
            Messages.Add(message);

            var messageToPeer = new ImageIntroduceMessage(message.MessageID);
            Send(messageToPeer, 1);

            return message;
        }

        public void SendUpdatedImageMessage(string messageID, byte[] pictureBytes, string pictureExtension)
        {
            var messageToPeer = new UpdateImageMessage(messageID, pictureBytes, pictureExtension);
            Send(messageToPeer, 1);
        }

        public ImageMessageModel? AddIncomingMessage(ImageIntroduceMessage imageMessageFromPeer)
        {
            var message = new ImageMessageModel(imageMessageFromPeer);
            Messages.Add(message);
            _incomingMessages.Add(message);

            HasNewMessages = true;

            return message;
        }

        public async Task SetUpdatedImageInMessage(string messageID, byte[] pictureBytes, string pictureExtension)
        {
            if (pictureBytes == null ||
                pictureBytes.Length == 0 ||
                pictureExtension.Length == 0)
            {
                return;
            }

            try
            {
                var message = Messages.First(message => message.MessageID == messageID) as ImageMessageModel;
                if (message == null)
                {
                    Debug.WriteLine($"(SetUpdatedImageInMessage) Can't find message with ID {messageID}");

                    return;
                }

                var newMessagePicture = await ImageItem.SaveByteArrayAsImage(pictureBytes,
                    pictureExtension,
                    Constants.ImageMessageThumbnailSize.Item1,
                    Constants.ImageMessageThumbnailSize.Item2);

                if (newMessagePicture != null &&
                    await newMessagePicture.TryLoadImage())
                {
                    message.UpdateImage(newMessagePicture);
                }
                else
                {
                    message.SetAsFailed();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"(SetUpdatedImageInMessage) Can't find message with ID {messageID}: {ex}");
            }
        }

        public void SendMessageAboutFailedImageSending(string messageID)
        {
            var errorMessage = new ImageSendingFailedMessage(messageID);
            Send(errorMessage);
        }

        public void SetImageMessageAsFailed(string messageID)
        {
            try
            {
                var message = Messages.First(message => message.MessageID == messageID) as ImageMessageModel;
                if (message == null)
                {
                    return;
                }

                message.SetAsFailed();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}