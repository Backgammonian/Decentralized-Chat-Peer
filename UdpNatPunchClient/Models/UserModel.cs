using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Meziantou.Framework.WPF.Collections;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class UserModel : ObservableObject
    {
        private readonly EncryptedPeer _peer;

        public UserModel(EncryptedPeer peer, string id)
        {
            _peer = peer;
            ID = id;
            Messages = new ConcurrentObservableCollection<MessageModel>();
        }

        public int PeerID => _peer.Id;
        public string EndPoint => _peer.EndPoint.ToString();
        public DateTime ConnectionTime => _peer.StartTime;
        public string ID { get; }
        public ConcurrentObservableCollection<MessageModel> Messages { get; }

        public void Send(BaseMessage message)
        {
            _peer.SendEncrypted(message);
        }

        public void MarkMessageAsDelivered(string messageID)
        {
            try
            {
                var message = Messages.First(message => message.MessageID == messageID);
                message.MarkAsDelivered();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
