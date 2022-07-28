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

        public PeerModel(EncryptedPeer peer)
        {
            _peer = peer;
            Messages = new ConcurrentObservableCollection<MessageModel>();
        }

        public int PeerID => _peer.Id;
        public string EndPoint => _peer.EndPoint.ToString();
        public ConcurrentObservableCollection<MessageModel> Messages { get; }

        public virtual void Send(BaseMessage message)
        {
            throw new NotImplementedException();
        }

        public virtual void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
