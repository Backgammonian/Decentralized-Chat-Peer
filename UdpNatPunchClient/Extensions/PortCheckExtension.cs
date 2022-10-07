using System.Linq;
using System.Net.NetworkInformation;

namespace Extensions
{
    public static class PortCheckExtension
    {
        public static bool IsPortOccupied(this int port)
        {
            return port > 0 &&
                port < 65536 &&
                IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(p => p.Port == port);
        }
    }
}
