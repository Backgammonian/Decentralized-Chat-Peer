﻿using System.Windows.Media.Imaging;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class UserModel : PeerModel
    {
        private string _nickname = string.Empty;
        private string _pictureBase64 = string.Empty;
        private ImageItem? _picture;

        public UserModel(EncryptedPeer peer, string id, string nickname, string pictureBase64) : base(peer)
        {
            ID = id;
            Nickname = nickname;
            PictureBase64 = pictureBase64;
        }

        public string ID { get; }
        public ImageItem? Picture => _picture;

        public string Nickname
        {
            get => _nickname;
            private set => SetProperty(ref _nickname, value);
        }

        public string PictureBase64
        {
            get => _pictureBase64;
            private set
            {
                if (value.TryConvertBase64ToBitmapImage(out var picture))
                {
                    SetProperty(ref _pictureBase64, value);

                    _picture = picture;
                    OnPropertyChanged(nameof(Picture));
                }
            }
        }

        public void UpdatePersonalInfo(string newNickname)
        {
            Nickname = newNickname;
        }

        public void UpdatePicture(string newPictureBase64)
        {
            PictureBase64 = newPictureBase64;
        }

        public void SendUpdatedPersonalInfo(string updatedNickname)
        {
            var updateMessage = new UpdatedInfoForPeerMessage(updatedNickname);
            Send(updateMessage);
        }

        public void SendUpdatedProfilePicture(string updatedPictureBase64)
        {
            var updateMessage = new UpdatedProfilePictureForPeerMessage(updatedPictureBase64);
            Send(updateMessage);
        }
    }
}
