using System.Net;

namespace InputBox
{
    public static class InputBoxExtensions
    {
        public static bool AskServerAddress(this InputBoxWindow window, string title, string question, IPAddress defaultAddress, out IPAddress? address)
        {
            address = null;

            window.TitleText = title;
            window.QuestionText = question;
            window.AnswerText = defaultAddress.ToString();

            if (window.ShowDialog() == true)
            {
                if (IPAddress.TryParse(window.AnswerText, out IPAddress? ip) &&
                    ip != null)
                {
                    address = ip;

                    return true;
                }
            }

            return false;
        }

        public static bool AskPort(this InputBoxWindow window, string title, string question, int defaultPort, out int port)
        {
            port = 0;

            window.TitleText = title;
            window.QuestionText = question;
            window.AnswerText = defaultPort + string.Empty;

            if (window.ShowDialog() == true)
            {
                if (int.TryParse(window.AnswerText, out int portNumber) &&
                    portNumber > 1024 &&
                    portNumber < 65536)
                {
                    port = portNumber;

                    return true;
                }
            }

            return false;
        }

        public static bool AskServerAddressAndPort(this InputBoxWindow window, string title, string question, IPEndPoint defaultEndPoint, out IPEndPoint? serverAddress)
        {
            serverAddress = null;

            window.TitleText = title;
            window.QuestionText = question;
            window.AnswerText = defaultEndPoint + string.Empty;

            if (window.ShowDialog() == true)
            {
                if (IPAddress.TryParse(window.AnswerText, out IPAddress? address) &&
                    address != null)
                {
                    serverAddress = new IPEndPoint(address, 55000);

                    return true;
                }

                if (IPEndPoint.TryParse(window.AnswerText, out IPEndPoint? endPoint) &&
                    endPoint != null &&
                    endPoint.Port > 1024)
                {
                    serverAddress = endPoint;

                    return true;
                }
            }

            return false;
        }
    }
}
