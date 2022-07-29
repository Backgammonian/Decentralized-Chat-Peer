using System;
using System.Diagnostics;
using System.Linq;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class TrackerModel : PeerModel
    {
        public TrackerModel(EncryptedPeer peer) : base(peer)
        {
        }

        public override void Send(BaseMessage baseMessage)
        {
            _peer.SendEncrypted(baseMessage);
        }

        public override void SendTextMessage(MessageModel textMessage)
        {
            Messages.Add(textMessage);

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
                var message = Messages.First(message => message.MessageID == messageID);
                message.MarkAsDelivered();
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
                var message = Messages.First(message => message.MessageID == messageID);
                message.MarkAsDelivered();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override void SendNotificationsToAllUnreadIncomingMessages()
        {
            //TODO
        }

        public override MessageModel AddIncomingMessage(TextMessageToPeer textMessageFromPeer)
        {
            var message = new MessageModel(textMessageFromPeer);
            Messages.Add(message);

            return message;
        }
    }
}