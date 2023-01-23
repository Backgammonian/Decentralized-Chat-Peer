using FluentAssertions;
using Xunit;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NetworkingLib.Tests
{
    public class Tests
    {
        [Fact]
        public async Task PeerToPeerConnection_ReturnSuccess()
        {
            var alice = new Client();
            var bob = new Client();

            alice.StartListening();
            bob.StartListening();

            alice.ConnectToPeer(new IPEndPoint(IPAddress.Loopback, bob.LocalPort));
            bob.ConnectToPeer(new IPEndPoint(IPAddress.Loopback, bob.LocalPort));

            await Task.Delay(200);

            var peer1 = alice.Peers.First();
            var peer2 = bob.Peers.First();

            peer1.Should().NotBeNull();
            peer2.Should().NotBeNull();
            peer1.IsSecurityEnabled.Should().BeTrue();
            peer2.IsSecurityEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task ClientToServerConnection_ReturnsSuccess()
        {
            var client = new Client();
            var server = new Server();

            client.StartListening();
            var serverPort = 60000;
            server.StartListening(serverPort);

            client.ConnectToServer(new IPEndPoint(IPAddress.Loopback, serverPort));

            await Task.Delay(200);

            var connectedServer = client.Server;
            var connectedClient = server.Clients.First();

            connectedClient.Should().NotBeNull();
            connectedServer.Should().NotBeNull();
            connectedClient.IsSecurityEnabled.Should().BeTrue();
            connectedServer.IsSecurityEnabled.Should().BeTrue();
        }
    }
}
