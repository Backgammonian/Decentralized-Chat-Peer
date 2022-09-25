using System;
using System.IO;
using Microsoft.Win32;

namespace Helpers
{
    public static class FileSelector
    {
        public static bool TrySelectFile(string title, string filter, out string path)
        {
            path = string.Empty;

            var selectFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            if (selectFileDialog.ShowDialog() == true)
            {
                path = selectFileDialog.FileName;

                return true;
            }

            return false;
        }

        public static bool TryGetFileSize(string path, out long size)
        {
            size = 0;

            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                size = stream.Length;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
