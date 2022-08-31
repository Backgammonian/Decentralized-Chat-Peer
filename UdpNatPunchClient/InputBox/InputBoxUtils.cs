using System.Net;

namespace InputBox
{
    public class InputBoxUtils
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

        public bool AskServerAddressAndPort(out IPEndPoint? serverAddress)
        {
            var inputBox = new InputBoxWindow(
                "Connection to Tracker",
                "Enter IPv4 address of Tracker (example: 10.0.0.8).\nAlso you can specify port (example: 10.0.0.8:55000)");

            if (inputBox.ShowDialog() == true)
            {
                if (IPAddress.TryParse(inputBox.Answer, out IPAddress? address) &&
                    address != null)
                {
                    serverAddress = new IPEndPoint(address, 55000);

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

        public bool AskIDOfDesiredUser(out string id)
        {
            var inputBox = new InputBoxWindow(
                "Bootstrap to User",
                "Enter ID of desired user");

            if (inputBox.ShowDialog() == true &&
                inputBox.Answer.IsNotEmpty())
            {
                id = inputBox.Answer;

                return true;
            }

            id = string.Empty;

            return false;
        }
    }
}
