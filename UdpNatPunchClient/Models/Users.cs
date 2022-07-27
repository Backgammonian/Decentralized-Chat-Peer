using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Networking;

namespace UdpNatPunchClient.Models
{
    public class Users
    {
        //user's id, UserModel object
        private readonly ConcurrentDictionary<string, UserModel> _users;

        public Users()
        {
            _users = new ConcurrentDictionary<string, UserModel>();
        }

        public event EventHandler<EventArgs>? UserAdded;
        public event EventHandler<EventArgs>? UserRemoved;

        public IEnumerable<UserModel> List => _users.Values;

        public UserModel? GetUserByPeer(EncryptedPeer peer)
        {
            try
            {
                return _users.Values.First(user => user.PeerID == peer.Id);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        
        public bool Has(string id)
        {
            return _users.ContainsKey(id);
        }

        public void Add(string id, EncryptedPeer peer)
        {
            var userModel = new UserModel(peer, id);
            if (!Has(id) &&
                _users.TryAdd(id, userModel))
            {
                UserAdded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Remove(string id)
        {
            if (Has(id) &&
                _users.TryRemove(id, out UserModel? removedUser) &&
                removedUser != null)
            {
                UserRemoved?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
