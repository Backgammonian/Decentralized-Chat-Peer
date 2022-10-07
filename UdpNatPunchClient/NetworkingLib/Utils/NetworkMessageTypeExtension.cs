using System;
using NetworkingLib.Messages;

namespace NetworkingLib.Utils
{
    public static class NetworkMessageTypeExtension
    {
        public static bool TryParseType(this byte typeByte, out NetworkMessageType type)
        {
            if (Enum.TryParse(typeByte + "", out NetworkMessageType messageType))
            {
                type = messageType;

                return true;
            }
            else
            {
                type = NetworkMessageType.Empty;

                return false;
            }
        }
    }
}
