using System;
using System.Diagnostics;
using System.Linq;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class UserModel : PeerModel
    {
        private bool _hasNewMessages;

        public UserModel(EncryptedPeer peer, string id) : base(peer)
        {
            ID = id;
        }

        public DateTime ConnectionTime => _peer.StartTime;
        public string ID { get; }

        public bool HasNewMessages
        {
            get => _hasNewMessages;
            private set => SetProperty(ref _hasNewMessages, value);
        }

        public override void Send(BaseMessage baseMessage)
        {
            _peer.SendEncrypted(baseMessage);
        }

        public override void SendTextMessage(MessageModel textMessage)
        {
            Messages.Add(textMessage);
            _undeliveredMessages.Add(textMessage);
            _unreadMessages.Add(textMessage);

            var textMessageToPeer = new TextMessageToPeer(textMessage.MessageID, textMessage.Content, textMessage.AuthorID);
            Send(textMessageToPeer);
        }

        public override void Disconnect()
        {
            _peer.Disconnect();
        }

        public override void MarkMessageAsDelivered(string messageID)
        {
            try
            {
                var message = _undeliveredMessages.First(message => message.MessageID == messageID);
                message.MarkAsDelivered();

                _undeliveredMessages.Remove(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override void MarkMessageAsReadAndDelivered(string messageID)
        {
            try
            {
                var message = _unreadMessages.First(message => message.MessageID == messageID);
                message.MarkAsReadAndDelivered();

                _undeliveredMessages.Remove(message);
                _unreadMessages.Remove(message);

                if (_unreadMessages.Count == 0)
                {
                    HasNewMessages = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override void SendNotificationsToAllUnreadIncomingMessages()
        {
            foreach (var incomingMessage in _incomingMessages)
            {
                SendReadNotification(incomingMessage);
            }

            HasNewMessages = false;
        }

        public override MessageModel AddIncomingMessage(TextMessageToPeer textMessageFromPeer)
        {
            var message = new MessageModel(textMessageFromPeer);
            Messages.Add(message);
            _incomingMessages.Add(message);

            HasNewMessages = true;

            return message;
        }


        public void SendReceiptNotification(MessageModel message)
        {
            var receiptNotification = new MessageReceiptNotification(message.MessageID);
            Send(receiptNotification);
        }

        public void SendReadNotification(MessageModel message)
        {
            var readNotification = new MessageReadNotification(message.MessageID);
            Send(readNotification);

            _incomingMessages.Remove(message);
        }

        public void DismissNewMessagesSignal()
        {
            HasNewMessages = false;
        }
    }
}
