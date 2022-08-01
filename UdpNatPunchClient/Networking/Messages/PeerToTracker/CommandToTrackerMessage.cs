using Networking.Utils;

namespace Networking.Messages
{
    public sealed class CommandToTrackerMessage : BaseMessage
    {
        public CommandToTrackerMessage(string command, string argument)
        {
            Type = NetworkMessageType.CommandToTracker;
            CommandID = RandomGenerator.GetRandomString(21);
            Command = command;
            Argument = argument;
        }

        public string CommandID { get; }
        public string Command { get; }
        public string Argument { get; }
    }
}