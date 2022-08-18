using System;

namespace UdpNatPunchClient.Models
{
    public class UserUpdatedEventArgs : EventArgs
    {
        public UserUpdatedEventArgs(UserModel user)
        {
            User = user;
        }

        public UserModel User { get; }
    }
}
