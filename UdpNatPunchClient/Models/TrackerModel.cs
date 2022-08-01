using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class TrackerModel : PeerModel
    {
        //commandID, message
        private readonly Dictionary<string, MessageModel> _commandsAndMessagesAccordance;

        public TrackerModel(EncryptedPeer peer) : base(peer)
        {
            _commandsAndMessagesAccordance = new Dictionary<string, MessageModel>();
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
            var commandMessage = new CommandToTrackerMessage(command, argument);
            var message = new MessageModel(string.Format("/{0} {1}", command, argument), MessageDirection.Outcoming);

            _commandsAndMessagesAccordance.Add(commandMessage.Command, message);
            Messages.Add(message);

            Send(commandMessage);
        }

        public void PrintInfo(string info)
        {
            Messages.Add(new MessageModel(info, MessageDirection.Incoming));
        }

        public void MarkCommandAsReadAndDelivered(string commandID)
        {
            if (!_commandsAndMessagesAccordance.ContainsKey(commandID))
            {
                return;
            }

            try
            {
                var messageID = _commandsAndMessagesAccordance[commandID].MessageID;

                var message = Messages.First(message => message.MessageID == messageID);
                message.MarkAsReadAndDelivered();

                _commandsAndMessagesAccordance.Remove(commandID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}