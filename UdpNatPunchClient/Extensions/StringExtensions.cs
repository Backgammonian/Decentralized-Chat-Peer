namespace Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string input)
        {
            return input == null ||
                string.IsNullOrEmpty(input) ||
                string.IsNullOrWhiteSpace(input);
        }

        public static bool IsNotEmpty(this string input)
        {
            return !IsEmpty(input);
        }
    }
}
