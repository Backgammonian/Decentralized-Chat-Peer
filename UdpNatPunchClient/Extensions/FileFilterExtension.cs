namespace Extensions
{
    public static class FileFilterExtension
    {
        private const string _allFiles = "All files (*.*)|*.*";

        public static string GetAppropriateFileFilter(this string fileExtension)
        {
            var fileFormat = fileExtension.Remove(0, 1).ToUpper();
            if (fileExtension.Length > 0)
            {
                return $"{fileFormat} files (*{fileExtension})|*{fileExtension}|{_allFiles}";
            }
            else
            {
                return _allFiles;
            }
        }
    }
}