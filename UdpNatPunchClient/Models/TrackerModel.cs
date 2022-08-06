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

        public void SendIntroductionMessage(string id, string nickname)
        {
            var introductionMessage = new IntroduceClientToTrackerMessage(id, nickname);
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

        public void PrintSupport(string currentMessage)
        {
            PrintInfo(string.Format("Not valid command input: {0}\nPrint '/help' (without quotes) to get list of commands", currentMessage));
        }

        public void PrintHelp()
        {
            var help = "List of commands:\n";
            help += "/connect [ID] - establish connection to peer with specified ID\n";
            help += "/connect [Nickname] - get list of users with such nickname\n";
            help += "/ping - get pong from tracker\n";
            help += "/time - get tracker's current time";

            PrintInfo(help);
        }

        public void PrintListOfUsers(UserInfoFromTracker[] users)
        {
            var i = 1;
            var response = "Response from tracker:\n";
            foreach (var user in users)
            {
                response += string.Format("{0}. Nickname: '{1}', ID: {2}\n", i, user.Nickname, user.ID);
                i += 1;
            }

            PrintInfo(response);
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

        public void SendUpdatedPersonalInfo(string newNickname)
        {
            var updatedInfoMessage = new UpdatedInfoToTrackerMessage(newNickname);
            Send(updatedInfoMessage);
        }
    }
}