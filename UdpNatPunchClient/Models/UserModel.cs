using Networking;

namespace UdpNatPunchClient.Models
{
    public class UserModel : PeerModel
    {
        public UserModel(EncryptedPeer peer, string id) : base(peer)
        {
            ID = id;
        }

        public string ID { get; }
    }
}
