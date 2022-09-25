using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Networking;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class Users
    {
        //user's id, UserModel object
        private readonly ConcurrentDictionary<string, UserModel> _users;

        public Users()
        {
            _users = new ConcurrentDictionary<string, UserModel>();
        }

        public event EventHandler<UserUpdatedEventArgs>? UserAdded;
        public event EventHandler<UserUpdatedEventArgs>? UserRemoved;

        public IEnumerable<UserModel> List => _users.Values.OrderBy(user => user.ConnectionTime);

        public UserModel? GetUserByPeer(EncryptedPeer peer)
        {
            try
            {
                return _users.Values.First(user => user.PeerID == peer.Id);
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(GetUserByPeer) Unknown user");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return null;
        }
        
        public bool Has(string id)
        {
            return _users.ContainsKey(id);
        }

        public async Task Add(string id, string nickname, byte[] profilePictureArray, string profilePictureExtension, EncryptedPeer peer)
        {
            var user = new UserModel(peer, id, nickname);

            if (!Has(user.ID) &&
                _users.TryAdd(user.ID, user))
            {
                UserAdded?.Invoke(this, new UserUpdatedEventArgs(user));
                await user.TrySetUpdatedPicture(profilePictureArray, profilePictureExtension);
            }
        }

        public async Task Add(IntroducePeerToPeerMessage message, EncryptedPeer peer)
        {
            await Add(message.ID, message.Nickname, message.PictureByteArray, message.PictureExtension, peer);
        }

        public async Task Add(IntroducePeerToPeerResponse message, EncryptedPeer peer)
        {
            await Add(message.ID, message.Nickname, message.PictureByteArray, message.PictureExtension, peer);
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

        public void SendKeepAliveMessageToConnectedUsers()
        {
            foreach (var user in _users.Values)
            {
                user.SendKeepAliveMessage();
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
