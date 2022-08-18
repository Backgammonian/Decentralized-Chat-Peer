using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Networking;
using Networking.Messages;

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

        public event EventHandler<UserUpdatedEventArgs>? UserAdded;
        public event EventHandler<UserUpdatedEventArgs>? UserRemoved;

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

        public void Add(string id, string nickname, byte[] profilePictureArray, string profilePictureExtension, EncryptedPeer peer)
        {
            if (!Has(id))
            {
                var user = new UserModel(peer, id, nickname);
                user.GetUpdatedPicture(profilePictureArray, profilePictureExtension);

                if (_users.TryAdd(id, user))
                {
                    UserAdded?.Invoke(this, new UserUpdatedEventArgs(user));
                }
            }
        }

        public void Add(IntroducePeerToPeerMessage message, EncryptedPeer peer)
        {
            Add(message.ID, message.Nickname, message.PictureByteArray, message.PictureExtension, peer);
        }

        public void Add(IntroducePeerToPeerResponse message, EncryptedPeer peer)
        {
            Add(message.ID, message.Nickname, message.PictureByteArray, message.PictureExtension, peer);
        }

        public void Remove(string id)
        {
            if (Has(id) &&
                _users.TryRemove(id, out UserModel? removedUser) &&
                removedUser != null)
            {
                UserRemoved?.Invoke(this, new UserUpdatedEventArgs(removedUser));
            }
        }

        public void SendUpdatedInfoToConnectedUsers(string updatedNickname)
        {
            foreach (var user in _users.Values)
            {
                user.SendUpdatedPersonalInfo(updatedNickname);
            }
        }

        public void SendUpdatedProfilePictureToConnectedUsers(byte[] updatedPictureArray, string updatedPictureExtension)
        {
            foreach (var user in _users.Values)
            {
                user.SendUpdatedProfilePicture(updatedPictureArray, updatedPictureExtension);
            }
        }
    }
}
