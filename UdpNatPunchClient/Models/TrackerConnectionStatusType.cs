namespace UdpNatPunchClient.Models
{
    public enum TrackerConnectionStatusType
    {
        None,
        TryingToConnect,
        FailedToConnect,
        Connected,
        DisconnectFromTracker
    }
}
