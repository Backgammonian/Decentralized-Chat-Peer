﻿using System;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using NetworkingLib;
using NetworkingLib.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class UserModel : PeerModel
    {
        private string _nickname = "Default nickname";
        private ImageItem? _picture;

        public UserModel(EncryptedPeer peer, string id, string nickname) : base(peer)
        {
            UserID = id;
            Nickname = nickname;
        }

        public string UserID { get; }

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
            Send(updateMessage, 0);
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
            Send(messageToPeer, 0);

            return message;
        }

        public void SendUpdatedImageMessage(string messageID, byte[] pictureBytes, string pictureExtension)
        {
            var messageToPeer = new UpdateImageMessage(messageID, pictureBytes, pictureExtension);
            Send(messageToPeer, 1);
        }

        public ImageMessageModel AddIncomingMessage(ImageIntroduceMessage imageMessageFromPeer)
        {
            var message = new ImageMessageModel(imageMessageFromPeer);
            Messages.Add(message);
            _incomingMessages.Add(message);

            HasNewMessages = true;

            return message;
        }

        public IncomingFileMessageModel AddIncomingMessage(FileMessage fileMessageFromPeer)
        {
            var message = new IncomingFileMessageModel(fileMessageFromPeer, this);
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
            Send(errorMessage, 0);
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

        public void SendFileMessage(SharedFileInfo sharedFileInfo)
        {
            var message = new OutgoingFileMessageModel(sharedFileInfo);
            _undeliveredMessages.Add(message);
            _unreadMessages.Add(message);
            Messages.Add(message);

            var messageToPeer = new FileMessage(sharedFileInfo.FileHash,
                sharedFileInfo.FileID,
                sharedFileInfo.Name,
                sharedFileInfo.Size,
                sharedFileInfo.NumberOfSegments,
                message.MessageID);
            Send(messageToPeer, 0);
        }

        public void SendFileRequest(Download download)
        {
            var messageToPeer = new FileRequestMessage(download.DownloadID,
                download.SharedFileID,
                download.OriginalName);
            Send(messageToPeer, 0);
        }

        public void SendFileSegmentAckMessage(string downloadID)
        {
            var segmentAckMessage = new FileSegmentAckMessage(downloadID);
            Send(segmentAckMessage, 0);
        }

        public void SendDownloadCancellationMessage(string downloadID)
        {
            var cancellationMessage = new DownloadCancellationMessage(downloadID);
            Send(cancellationMessage, 0);
        }

        public void SendUploadCancellationMessage(string uploadID)
        {
            var cancellationMessage = new UploadCancellationMessage(uploadID);
            Send(cancellationMessage, 0);
        }

        public void SendFileSegment(string uploadID, byte[] segment)
        {
            var fileSegmentMessage = new FileSegmentMessage(uploadID, segment);
            Send(fileSegmentMessage, 2);
        }

        public void SendFileRequestErrorMessage(string downloadID, string fileName)
        {
            var message = new FileRequestErrorMessage(downloadID, fileName);
            Send(message, 0);
        }

        public void SendFileIsNotAvailableMessage(string fileID)
        {
            var message = new FileIsNotAvailableMessage(fileID);
            Send(message, 0);
        }
    }
}