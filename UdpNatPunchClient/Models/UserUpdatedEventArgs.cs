using System;

namespace UdpNatPunchClient.Models
{
    public sealed class UserUpdatedEventArgs : EventArgs
    {
        public UserUpdatedEventArgs(UserModel user)
        {
            User = user;
        }

        public UserModel User { get; }
    }
}
