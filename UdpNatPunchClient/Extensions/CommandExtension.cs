namespace Extensions
{
    public static class CommandExtension
    {
        public static bool TryParseCommand(this string input, out string command, out string argument)
        {
            var spacePosition = input.IndexOf(' ');
            if (spacePosition == -1)
            {
                command = input.Substring(0, input.Length).ToLower();
                argument = string.Empty;
            }
            else
            {
                command = input.Substring(0, spacePosition).ToLower();
                argument = input.Substring(spacePosition + 1, input.Length - spacePosition - 1);
            }

            return true;
        }
    }
}
