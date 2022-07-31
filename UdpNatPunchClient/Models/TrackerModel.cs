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

        public override void SendTextMessage(MessageModel message)
        {
            //pass
        }

        public override MessageModel? AddIncomingMessage(TextMessageToPeer textMessageFromPeer)
        {
            //pass
            return null;
        }

        public void SendIntroductionMessage(string id)
        {
            var introductionMessage = new IntroduceClientToTrackerMessage(id);
            Send(introductionMessage);
        }

        public void SendCommandMessage(string command, string argument)
        {

        }
    }
}