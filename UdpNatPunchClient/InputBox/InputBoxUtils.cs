using System.Net;

namespace InputBox
{
    public sealed class InputBoxUtils
    {
        public bool AskServerAddress(out IPAddress? address)
        {
            var inputBox = new InputBoxWindow(
                "Connection to Tracker",
                "Enter IP address of Tracker");

            if (inputBox.ShowDialog() == true)
            {
                if (IPAddress.TryParse(inputBox.Answer, out IPAddress? ip) &&
                    ip != null)
                {
                    address = ip;

                    return true;
                }
            }

            address = null;

            return false;
        }

        public bool AskServerAddressAndPort(IPAddress defaultAddress, int defaultPort, out IPEndPoint? serverAddress)
        {
            var inputBox = new InputBoxWindow(
                "Connection to Tracker",
                $"Enter IPv4 address of Tracker (example: {defaultAddress}).\nAlso you can specify port (example: {defaultAddress}:{defaultPort})",
                $"{defaultAddress}:{defaultPort}");

            if (inputBox.ShowDialog() == true)
            {
                if (IPAddress.TryParse(inputBox.Answer, out IPAddress? address) &&
                    address != null)
                {
                    serverAddress = new IPEndPoint(address, defaultPort);

                    return true;
                }

                if (IPEndPoint.TryParse(inputBox.Answer, out IPEndPoint? endPoint) &&
                    endPoint != null &&
                    endPoint.Port > 1024)
                {
                    serverAddress = endPoint;

                    return true;
                }
            }

            serverAddress = null;

            return false;
        }
    }
}
