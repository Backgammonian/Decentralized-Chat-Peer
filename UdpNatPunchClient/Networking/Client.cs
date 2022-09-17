using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using LiteNetLib;
using LiteNetLib.Layers;
using Networking.Messages;

namespace Networking
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
            _xor = new XorEncryptLayer("VerySecretSymmetricXorPassword");
            _client = new NetManager(_listener, _xor);
            _client.ChannelsCount = NetworkingConstants.ChannelsCount;
            _client.DisconnectTimeout = NetworkingConstants.DisconnectionTimeoutMilliseconds;
            _peers = new EncryptedPeers();
            _peers.PeerAdded += OnPeerAdded;
            _peers.PeerRemoved += OnPeerRemoved;
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            _listenTask = new Task(async () => await Run(token));

            ExpectedTracker = null;
            Tracker = null;
        }

        public event AsyncEventHandler<NetEventArgs>? MessageFromPeerReceived;
        public event EventHandler<EncryptedPeerEventArgs>? PeerAdded;
        public event EventHandler<EncryptedPeerEventArgs>? PeerConnected;
        public event EventHandler<EncryptedPeerEventArgs>? PeerRemoved;
        public event EventHandler<NetEventArgs>? MessageFromTrackerReceived;
        public event EventHandler<EventArgs>? TrackerAdded;
        public event EventHandler<EventArgs>? TrackerConnected;
        public event EventHandler<TrackerDisconnectedEventArgs>? TrackerRemoved;
        public event EventHandler<EventArgs>? TrackerConnectionAttemptFailed;

        public int LocalPort => _client.LocalPort;
        public byte ChannelsCount => _client.ChannelsCount;
        public IEnumerable<EncryptedPeer> Peers => _peers.List;
        public EncryptedPeer? Tracker { get; private set; }
        public IPEndPoint? ExpectedTracker { get; private set; }

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

        public bool IsConnectedToPeer(int peerID, out EncryptedPeer? peer)
        {
            peer = null;

            if (_peers.Has(peerID) &&
                _peers[peerID].IsSecurityEnabled)
            {
                peer = _peers[peerID];

                return true;
            }

            return false;
        }

        public bool IsConnectedToTracker(IPEndPoint trackerAddress)
        {
            return Tracker != null &&
                Tracker.EndPoint == trackerAddress;
        }

        public EncryptedPeer? GetPeerByID(int peerID)
        {
            return _peers.Has(peerID) ? _peers[peerID] : null;
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            _client.Stop();
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

            _client.Connect(peerAddress, "ToChatPeer");
        }

        public void ConnectToTracker(IPEndPoint trackerAddress)
        {
            if (Tracker != null)
            {
                var endPoint = Tracker.EndPoint;

                Tracker.Disconnect();
                TrackerRemoved?.Invoke(this, new TrackerDisconnectedEventArgs(endPoint));
                Tracker = null;
            }

            ExpectedTracker = trackerAddress;
            _client.Connect(ExpectedTracker, "ToChatTracker");

            Debug.WriteLine($"Expecting tracker: {ExpectedTracker}");
        }

        public void StartListening()
        {
            _client.Start();

            _listener.ConnectionRequestEvent += request => request.AcceptIfKey("ToChatPeer");

            _listener.PeerConnectedEvent += peer =>
            {
                if (ExpectedTracker == peer.EndPoint)
                {
                    Debug.WriteLine("Expected tracker connected");

                    ExpectedTracker = null;
                    Tracker = new EncryptedPeer(peer);
                    Tracker.SendPublicKeys();

                    TrackerAdded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var newPeer = new EncryptedPeer(peer);
                    _peers.Add(newPeer);
                    _peers[newPeer.Id].SendPublicKeys();
                }
            };

            _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                if (Tracker != null &&
                    Tracker.Id == peer.Id)
                {
                    Debug.WriteLine($"(Client) Tracker {peer.EndPoint} disconnected");

                    Tracker = null;
                    TrackerRemoved?.Invoke(this, new TrackerDisconnectedEventArgs(peer.EndPoint));

                    if (ExpectedTracker == peer.EndPoint)
                    {
                        ExpectedTracker = null;
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

                if (ExpectedTracker == peer.EndPoint)
                {
                    TrackerConnectionAttemptFailed?.Invoke(this, EventArgs.Empty);
                }
            };

            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                if (Tracker != null &&
                    Tracker.Id == fromPeer.Id) //from tracker
                {
                    if (Tracker.IsSecurityEnabled &&
                        Tracker.TryDecryptReceivedData(dataReader, out NetworkMessageType type, out string json))
                    {
                        MessageFromTrackerReceived?.Invoke(this, new NetEventArgs(Tracker, type, json));
                    }
                    else
                    if (!Tracker.IsSecurityEnabled)
                    {
                        if (dataReader.TryGetBytesWithLength(out byte[] publicKey) &&
                            dataReader.TryGetBytesWithLength(out byte[] signaturePublicKey) &&
                            dataReader.TryGetULong(out ulong recepientsIncomingSegmentNumber))
                        {
                            Tracker.ApplyKeys(publicKey, signaturePublicKey, recepientsIncomingSegmentNumber);

                            TrackerConnected?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
                else
                if (_peers.Has(fromPeer.Id)) //from peer
                {
                    if (_peers[fromPeer.Id].IsSecurityEnabled &&
                        _peers[fromPeer.Id].TryDecryptReceivedData(dataReader, out NetworkMessageType type, out string json))
                    {
                        MessageFromPeerReceived?.Invoke(this, new NetEventArgs(_peers[fromPeer.Id], type, json));
                    }
                    else
                    if (!_peers[fromPeer.Id].IsSecurityEnabled)
                    {
                        if (dataReader.TryGetBytesWithLength(out byte[] publicKey) &&
                            dataReader.TryGetBytesWithLength(out byte[] signaturePublicKey) &&
                            dataReader.TryGetULong(out ulong recepientsIncomingSegmentNumber))
                        {
                            _peers[fromPeer.Id].ApplyKeys(publicKey, signaturePublicKey, recepientsIncomingSegmentNumber);

                            PeerConnected?.Invoke(this, new EncryptedPeerEventArgs(_peers[fromPeer.Id]));
                        }
                    }
                }

                dataReader.Recycle();
            };

            _listenTask.Start();
        }
    }
}
