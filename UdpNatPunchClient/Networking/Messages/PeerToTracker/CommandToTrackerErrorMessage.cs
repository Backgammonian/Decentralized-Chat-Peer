namespace Networking.Messages
{
    public sealed class CommandToTrackerErrorMessage : BaseMessage
    {
        public CommandToTrackerErrorMessage(string wrongCommand, string argument)
        {
            Type = NetworkMessageType.CommandToTrackerError;
            WrongCommand = wrongCommand;
            Argument = argument;
        }

        public string WrongCommand { get; }
        public string Argument { get; }
    }
}
