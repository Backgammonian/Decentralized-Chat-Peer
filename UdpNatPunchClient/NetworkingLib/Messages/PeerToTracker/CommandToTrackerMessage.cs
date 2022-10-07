using NetworkingLib.Utils;

namespace NetworkingLib.Messages
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

        public string CommandID { get; set; }
        public string Command { get; set; }
        public string Argument { get; set; }
    }
}