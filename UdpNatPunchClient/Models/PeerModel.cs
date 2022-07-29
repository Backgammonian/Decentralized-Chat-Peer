using System;
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

        public PeerModel(EncryptedPeer peer)
        {
            _peer = peer;
            _undeliveredMessages = new ConcurrentObservableCollection<MessageModel>();
            _unreadMessages = new ConcurrentObservableCollection<MessageModel>();
            _incomingMessages = new ConcurrentObservableCollection<MessageModel>();
            Messages = new ConcurrentObservableCollection<MessageModel>();
        }

        public int PeerID => _peer.Id;
        public string EndPoint => _peer.EndPoint.ToString();
        public ConcurrentObservableCollection<MessageModel> Messages { get; }

        public virtual void Send(BaseMessage baseMessage)
        {
            throw new NotImplementedException();
        }

        public virtual void SendTextMessage(MessageModel message)
        {
            throw new NotImplementedException();
        }

        public virtual void Disconnect()
        {
            throw new NotImplementedException();
        }

        public virtual void MarkMessageAsDelivered(string messageID)
        {
            throw new NotImplementedException();
        }

        public virtual void MarkMessageAsReadAndDelivered(string messageID)
        {
            throw new NotImplementedException();
        }

        public virtual void SendNotificationsToAllUnreadIncomingMessages()
        {
            throw new NotImplementedException();
        }

        public virtual MessageModel AddIncomingMessage(TextMessageToPeer textMessageFromPeer)
        {
            throw new NotImplementedException();
        }
    }
}
