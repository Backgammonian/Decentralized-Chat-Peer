using System;
using System.Diagnostics;
using System.Linq;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class UserModel : PeerModel
    {
        public UserModel(EncryptedPeer peer, string id) : base(peer)
        {
            ID = id;
        }

        public DateTime ConnectionTime => _peer.StartTime;
        public string ID { get; }

        public override void Send(BaseMessage message)
        {
            _peer.SendEncrypted(message);
        }

        public override void Disconnect()
        {
            _peer.Disconnect();
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
