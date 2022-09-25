namespace Extensions
{
    public static class FileFilterExtension
    {
        public static string GetAppropriateFileFilter(this string fileExtension)
        {
            var fileFormat = fileExtension.Remove(0, 1).ToUpper();
            if (fileExtension.Length > 0)
            {
                return $"{fileFormat} files (*{fileExtension})|*{fileExtension}|All files (*.*)|*.*";
            }
            else
            {
                return "All files (*.*)|*.*";
            }
        }
    }
}