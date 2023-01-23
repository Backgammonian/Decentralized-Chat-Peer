using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

namespace NetworkingLib
{
    public sealed class EncryptedPeers : IDisposable
    {
        private readonly ConcurrentDictionary<int, EncryptedPeer> _encryptedPeers;
        private bool _isDisposed;

        public EncryptedPeers()
        {
            _encryptedPeers = new ConcurrentDictionary<int, EncryptedPeer>();
        }

        public event EventHandler<EncryptedPeerEventArgs>? PeerAdded;
        public event EventHandler<EncryptedPeerEventArgs>? PeerRemoved;

        public IEnumerable<EncryptedPeer> List =>
            _encryptedPeers.Values.OrderBy(peer => peer.StartTime);
        public IEnumerable<EncryptedPeer> EstablishedList =>
             _encryptedPeers.Values.Where(peer => peer.IsSecurityEnabled).OrderBy(peer => peer.StartTime);

        public EncryptedPeer? Get(int peerId)
        {
            if (Has(peerId) &&
                _encryptedPeers.TryGetValue(peerId, out var peer))
            {
                return peer;
            }

            return null;
        }

        public bool Has(int peerId)
        {
            return _encryptedPeers.ContainsKey(peerId);
        }

        public bool IsConnectedToEndPoint(IPEndPoint endPoint)
        {
            return _encryptedPeers.Values.Any(cryptoPeer => cryptoPeer.EndPoint.ToString() == endPoint.ToString());
        }

        public void Add(EncryptedPeer encryptedPeer)
        {
            if (!Has(encryptedPeer.Id) &&
                _encryptedPeers.TryAdd(encryptedPeer.Id, encryptedPeer))
            {
                _encryptedPeers[encryptedPeer.Id].PeerDisconnected += OnCryptoPeerDisconnected;
                PeerAdded?.Invoke(this, new EncryptedPeerEventArgs(encryptedPeer));
            }
        }

        public void Remove(int peerID)
        {
            if (Has(peerID) &&
                _encryptedPeers.TryRemove(peerID, out EncryptedPeer? removedPeer))
            {
                removedPeer.PeerDisconnected -= OnCryptoPeerDisconnected;
                PeerRemoved?.Invoke(this, new EncryptedPeerEventArgs(removedPeer));
                removedPeer.Dispose();
            }
        }

        private void OnCryptoPeerDisconnected(object? sender, EncryptedPeerEventArgs e)
        {
            PeerRemoved?.Invoke(this, e);
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    foreach (var keyPair in _encryptedPeers)
                    {
                        keyPair.Value.Dispose();
                    }
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}