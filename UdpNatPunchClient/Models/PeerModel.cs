using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Meziantou.Framework.WPF.Collections;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public abstract class PeerModel : ObservableObject
    {
        protected readonly EncryptedPeer _peer;
        protected readonly ConcurrentObservableCollection<MessageModel> _undeliveredMessages;
        protected readonly ConcurrentObservableCollection<MessageModel> _unreadMessages;
        protected readonly ConcurrentObservableCollection<MessageModel> _incomingMessages;
        protected bool _hasNewMessages;

        public PeerModel(EncryptedPeer peer)
        {
            _peer = peer;
            _undeliveredMessages = new ConcurrentObservableCollection<MessageModel>();
            _unreadMessages = new ConcurrentObservableCollection<MessageModel>();
            _incomingMessages = new ConcurrentObservableCollection<MessageModel>();
            Messages = new ConcurrentObservableCollection<BaseMessageModel>();
            EndPoint = _peer.EndPoint.ToString();
        }

        public int PeerID => _peer.Id;
        public DateTime ConnectionTime => _peer.StartTime;
        public int Ping => _peer.Ping;
        public string EndPoint { get; }
        public ConcurrentObservableCollection<BaseMessageModel> Messages { get; }

        public bool HasNewMessages
        {
            get => _hasNewMessages;
            protected set => SetProperty(ref _hasNewMessages, value);
        }

        protected virtual void Send(BaseMessage baseMessage)
        {
            _peer.SendEncrypted(baseMessage);
        }

        public virtual void Disconnect()
        {
            _peer.Disconnect();
        }

        public virtual void SendTextMessage(MessageModel message)
        {
            Messages.Add(message);
            _undeliveredMessages.Add(message);
            _unreadMessages.Add(message);

            var textMessageToPeer = new TextMessageToPeer(message.MessageID, message.Content, message.AuthorID);
            Send(textMessageToPeer);
        }

        public virtual void MarkMessageAsDelivered(string messageID)
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

        public virtual void MarkMessageAsReadAndDelivered(string messageID)
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

        public virtual void SendNotificationsToAllUnreadIncomingMessages()
        {
            foreach (var incomingMessage in _incomingMessages)
            {
                SendReadNotification(incomingMessage);
            }

            HasNewMessages = false;
        }

        public virtual MessageModel? AddIncomingMessage(TextMessageToPeer textMessageFromPeer)
        {
            var message = new MessageModel(textMessageFromPeer);
            Messages.Add(message);
            _incomingMessages.Add(message);

            HasNewMessages = true;

            return message;
        }

        public virtual void SendReceiptNotification(MessageModel message)
        {
            var receiptNotification = new MessageReceiptNotification(message.MessageID);
            Send(receiptNotification);
        }

        public virtual void SendReadNotification(MessageModel message)
        {
            var readNotification = new MessageReadNotification(message.MessageID);
            Send(readNotification);

            _incomingMessages.Remove(message);
        }

        public virtual void DismissNewMessagesSignal()
        {
            HasNewMessages = false;
        }
    }
}
