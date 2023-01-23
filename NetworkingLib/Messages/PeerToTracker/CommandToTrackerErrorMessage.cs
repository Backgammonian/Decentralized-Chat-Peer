namespace NetworkingLib.Messages
{
    public sealed class CommandToTrackerErrorMessage : BaseMessage
    {
        public CommandToTrackerErrorMessage(string wrongCommand, string argument)
        {
            Type = NetworkMessageType.CommandToTrackerError;
            WrongCommand = wrongCommand;
            Argument = argument;
        }

        public string WrongCommand { get; set; }
        public string Argument { get; set; }
    }
}
