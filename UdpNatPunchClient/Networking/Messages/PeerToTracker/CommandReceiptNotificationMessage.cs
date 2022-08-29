namespace Networking.Messages
{
    public sealed class CommandReceiptNotificationMessage : BaseMessage
    {
        public CommandReceiptNotificationMessage(string commandID)
        {
            Type = NetworkMessageType.CommandReceiptNotification;
            CommandID = commandID;
        }

        public string CommandID { get; set; }
    }
}
