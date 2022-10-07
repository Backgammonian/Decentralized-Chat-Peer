using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using LiteNetLib;
using LiteNetLib.Layers;
using NetworkingLib.Messages;
using NetworkingLib.Utils;

namespace NetworkingLib
{
    public sealed class Client
    {
        private readonly EventBasedNetListener _listener;
        private readonly XorEncryptLayer _xor;
        private readonly NetManager _client;
        private readonly EncryptedPeers _peers;
        private readonly Task _listenTask;
        private readonly CancellationTokenSource _tokenSource;

        public Client()
        {
            _listener = new EventBasedNetListener();
            _xor = new XorEncryptLayer("VerySecretSymmetricXorPassword3923");
            _client = new NetManager(_listener, _xor);
            _client.ChannelsCount = NetworkingConstants.ChannelsCount;
            _client.DisconnectTimeout = NetworkingConstants.DisconnectionTimeoutMilliseconds;
            _peers = new EncryptedPeers();
            _peers.PeerAdded += OnPeerAdded;
            _peers.PeerRemoved += OnPeerRemoved;
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            _listenTask = new Task(async () => await Run(token));

            ExpectedServer = null;
            Server = null;
        }

        public event EventHandler<EventArgs>? ServerAdded;
        public event EventHandler<EventArgs>? ServerConnected;
        public event EventHandler<ServerDisconnectedEventArgs>? ServerRemoved;
        public event EventHandler<ServerDisconnectedEventArgs>? ServerConnectionAttemptFailed;
        public event EventHandler<NetEventArgs>? MessageFromServerReceived;

        public event EventHandler<EncryptedPeerEventArgs>? PeerAdded;
        public event EventHandler<EncryptedPeerEventArgs>? PeerConnected;
        public event EventHandler<EncryptedPeerEventArgs>? PeerRemoved;
        public event AsyncEventHandler<NetEventArgs>? MessageFromPeerReceived;

        public int LocalPort => _client.LocalPort;
        public byte ChannelsCount => _client.ChannelsCount;
        public IEnumerable<EncryptedPeer> Peers => _peers.List;
        public EncryptedPeer? Server { get; private set; }
        public IPEndPoint? ExpectedServer { get; private set; }

        private void OnPeerAdded(object? sender, EncryptedPeerEventArgs e)
        {
            PeerAdded?.Invoke(this, e);
        }

        private void OnPeerRemoved(object? sender, EncryptedPeerEventArgs e)
        {
            PeerRemoved?.Invoke(this, e);
        }

        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _client.PollEvents();
                await Task.Delay(15);
            }
        }

        public EncryptedPeer? GetPeerByID(int peerID)
        {
            return _peers.Get(peerID);
        }

        public bool IsConnectedToServer(IPEndPoint serverAddress)
        {
            return Server != null &&
                Server.EndPoint == serverAddress;
        }

        public void RemovePeer(EncryptedPeer peer)
        {
            peer.Disconnect();
        }

        public void DisconnectAll()
        {
            _client.DisconnectAll();
        }

        public void ConnectToPeer(IPEndPoint peerAddress)
        {
            if (_peers.IsConnectedToEndPoint(peerAddress))
            {
                return;
            }

            _client.Connect(peerAddress, "ToPeer");
        }

        public void ConnectToServer(IPEndPoint serverAddress)
        {
            if (Server != null)
            {
                var endPoint = Server.EndPoint;

                Server.Disconnect();
                ServerRemoved?.Invoke(this, new ServerDisconnectedEventArgs(endPoint));
                Server = null;
            }

            ExpectedServer = serverAddress;
            _client.Connect(ExpectedServer, "ToServer");

            Debug.WriteLine($"(Client) Expecting tracker: {ExpectedServer}");
        }

        public void StartListening()
        {
            _client.Start();

            _listener.ConnectionRequestEvent += request => request.AcceptIfKey("ToPeer");

            _listener.PeerConnectedEvent += peer =>
            {
                if (ExpectedServer == peer.EndPoint)
                {
                    Debug.WriteLine($"(Client) Expected server {peer.EndPoint} has connected");

                    ExpectedServer = null;
                    Server = new EncryptedPeer(peer);
                    Server.SendPublicKeys();

                    ServerAdded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var newPeer = new EncryptedPeer(peer);
                    newPeer.SendPublicKeys();
                    _peers.Add(newPeer);
                }
            };

            _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                if (Server != null &&
                    Server.Id == peer.Id)
                {
                    Debug.WriteLine($"(Client) Server {peer.EndPoint} disconnected");

                    Server = null;
                    ServerRemoved?.Invoke(this, new ServerDisconnectedEventArgs(peer.EndPoint));

                    if (ExpectedServer == peer.EndPoint)
                    {
                        ExpectedServer = null;
                    }
                }
                else
                if (_peers.Has(peer.Id))
                {
                    Debug.WriteLine($"(Client) Peer {peer.EndPoint} disconnected");

                    _peers.Remove(peer.Id);
                }
                else
                {
                    Debug.WriteLine($"(Client) Someone ({peer.EndPoint}) disconnected");
                }

                if (ExpectedServer == peer.EndPoint)
                {
                    ServerConnectionAttemptFailed?.Invoke(this, new ServerDisconnectedEventArgs(peer.EndPoint));
                }
            };

            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var peer = _peers.Get(fromPeer.Id);

                if (Server != null &&
                    Server.Id == fromPeer.Id) //from server
                {
                    if (Server.IsSecurityEnabled &&
                        Server.TryDecryptReceivedData(dataReader, out NetworkMessageType type, out string json))
                    {
                        MessageFromServerReceived?.Invoke(this, new NetEventArgs(Server, type, json));
                    }
                    else
                    if (!Server.IsSecurityEnabled &&
                        dataReader.TryGetBytesWithLength(out byte[] publicKey) &&
                        dataReader.TryGetBytesWithLength(out byte[] signaturePublicKey))
                    {
                        var isKeyApplied = Server.ApplyKeys(publicKey, signaturePublicKey);

                        Debug.WriteLine($"(Client) Is key from server {Server.EndPoint} applied: {isKeyApplied}");

                        ServerConnected?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                if (peer != null) //from peer
                {
                    if (peer.IsSecurityEnabled &&
                       peer.TryDecryptReceivedData(dataReader, out NetworkMessageType type, out string json))
                    {
                        MessageFromPeerReceived?.Invoke(this, new NetEventArgs(peer, type, json));
                    }
                    else
                    if (!peer.IsSecurityEnabled &&
                        dataReader.TryGetBytesWithLength(out byte[] publicKey) &&
                        dataReader.TryGetBytesWithLength(out byte[] signaturePublicKey))
                    {
                        var isKeyApplied = peer.ApplyKeys(publicKey, signaturePublicKey);

                        Debug.WriteLine($"(Client) Is key from peer {peer.EndPoint} applied: {isKeyApplied}");

                        PeerConnected?.Invoke(this, new EncryptedPeerEventArgs(peer));
                    }
                }

                dataReader.Recycle();
            };

            _listenTask.Start();
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            _client.Stop();
        }
    }
}
