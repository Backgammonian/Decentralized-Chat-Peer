using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NetworkingLib;
using NetworkingLib.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class TrackerModel : PeerModel
    {
        private static readonly StringBuilder _helpStringBuilder = new StringBuilder()
            .AppendLine("List of commands:")
            .AppendLine("\tconnect [ID] - establish connection to peer with specified ID")
            .AppendLine("\tconnect [Nickname] - get list of users with such nickname")
            .AppendLine("\tping - get pong from tracker")
            .AppendLine("\ttime - get tracker's current time");

        private static readonly string _helpText = _helpStringBuilder.ToString();

        //commandID, messageID
        private readonly Dictionary<string, string> _commandsAndMessagesAccordance;

        public TrackerModel(EncryptedPeer peer) : base(peer)
        {
            _commandsAndMessagesAccordance = new Dictionary<string, string>();
        }

        public override void SendTextMessage(MessageModel message)
        {
            //pass
        }

        public override MessageModel AddIncomingMessage(TextMessageToPeer textMessageFromPeer)
        {
            //pass
            return new MessageModel(string.Empty);
        }

        public void SendIntroductionMessage(string id, string nickname)
        {
            var introductionMessage = new IntroduceClientToTrackerMessage(id, nickname);
            Send(introductionMessage, 0);
        }

        public void SendCommandMessage(string command, string argument)
        {
            var commandMessage = new CommandToTrackerMessage(command, argument);
            var message = new MessageModel(string.Format("{0} {1}", command, argument), MessageDirection.Outgoing);

            Debug.WriteLine($"(SendCommandMessage) CommandID: {commandMessage.CommandID}");
            Debug.WriteLine($"(SendCommandMessage) MessageID: {message.MessageID}");

            _commandsAndMessagesAccordance.Add(commandMessage.CommandID, message.MessageID);
            Messages.Add(message);

            Send(commandMessage, 0);
        }

        public void PrintSupport(string currentMessage)
        {
            PrintInfo(string.Format("Not valid command input: {0}\nPrint 'help' (without quotes) to get list of commands", currentMessage));
        }

        public void PrintHelp()
        {
            PrintInfo(_helpText);
        }

        public void PrintListOfUsers(List<UserInfoFromTracker> users)
        {
            var i = 1;
            var responseStringBuilder = new StringBuilder("Response from tracker:");
            foreach (var user in users)
            {
                responseStringBuilder.AppendLine($"{i}. Nickname: '{user.Nickname}', ID: {user.ID}");
                i += 1;
            }

            PrintInfo(responseStringBuilder.ToString());
        }

        public void MarkCommandAsReadAndDelivered(string commandID)
        {
            Debug.WriteLine($"(MarkCommandAsReadAndDelivered) CommandID: {commandID}");

            if (!_commandsAndMessagesAccordance.ContainsKey(commandID))
            {
                return;
            }

            try
            {
                var messageID = _commandsAndMessagesAccordance[commandID];

                Debug.WriteLine($"(MarkCommandAsReadAndDelivered) MessageID: {messageID}");

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
            Send(updatedInfoMessage, 0);
        }
    }
}