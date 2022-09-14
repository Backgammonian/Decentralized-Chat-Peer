using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections;
using System.Net;

namespace Networking
{
    public sealed class EncryptedPeers : IEnumerable<EncryptedPeer>
    {
        private readonly ConcurrentDictionary<int, EncryptedPeer> _encryptedPeers;

        public EncryptedPeers()
        {
            _encryptedPeers = new ConcurrentDictionary<int, EncryptedPeer>();
        }

        public event EventHandler<EncryptedPeerEventArgs>? PeerAdded;
        public event EventHandler<EncryptedPeerEventArgs>? PeerRemoved;

        public IEnumerable<EncryptedPeer> List => _encryptedPeers.Values;
        public IEnumerable<EncryptedPeer> EstablishedList => _encryptedPeers.Values.Where(cryptoPeer => cryptoPeer.IsSecurityEnabled);

        public EncryptedPeer this[int peerId]
        {
            get => _encryptedPeers[peerId];
            private set => _encryptedPeers[peerId] = value;
        }

        public bool Has(int peerId)
        {
            return _encryptedPeers.ContainsKey(peerId);
        }

        public bool Has(EncryptedPeer encryptedPeer)
        {
            return _encryptedPeers.ContainsKey(encryptedPeer.Id);
        }

        public bool IsConnectedToEndPoint(IPEndPoint endPoint)
        {
            return _encryptedPeers.Values.Any(cryptoPeer => cryptoPeer.EndPoint.ToString() == endPoint.ToString());
        }

        public void Add(EncryptedPeer encryptedPeer)
        {
            if (!Has(encryptedPeer) &&
                _encryptedPeers.TryAdd(encryptedPeer.Id, encryptedPeer))
            {
                _encryptedPeers[encryptedPeer.Id].PeerDisconnected += OnCryptoPeerDisconnected;

                PeerAdded?.Invoke(this, new EncryptedPeerEventArgs(encryptedPeer));
            }
        }

        public void Remove(EncryptedPeer encryptedPeer)
        {
            if (Has(encryptedPeer) &&
                _encryptedPeers.TryRemove(encryptedPeer.Id, out EncryptedPeer _))
            {
                encryptedPeer.PeerDisconnected -= OnCryptoPeerDisconnected;

                PeerRemoved?.Invoke(this, new EncryptedPeerEventArgs(encryptedPeer));
            }
        }

        public void Remove(int peerID)
        {
            if (Has(peerID) &&
                _encryptedPeers.TryRemove(peerID, out EncryptedPeer? removedPeer))
            {
                removedPeer.PeerDisconnected -= OnCryptoPeerDisconnected;

                PeerRemoved?.Invoke(this, new EncryptedPeerEventArgs(removedPeer));
            }
        }

        private void OnCryptoPeerDisconnected(object? sender, EncryptedPeerEventArgs e)
        {
            PeerRemoved?.Invoke(this, e);
        }

        public IEnumerator<EncryptedPeer> GetEnumerator()
        {
            return _encryptedPeers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_encryptedPeers.Values).GetEnumerator();
        }
    }
}