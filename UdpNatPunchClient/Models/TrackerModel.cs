using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class TrackerModel : PeerModel
    {
        public TrackerModel(EncryptedPeer peer) : base(peer)
        {
        }

        public override void Send(BaseMessage message)
        {
            _peer.SendEncrypted(message);
        }

        public override void Disconnect()
        {
            _peer.Disconnect();
        }
    }
}
