using System;
using System.IO;
using System.Security.Cryptography;

namespace NetworkingLib.Cryptography
{
    public static class FileHashHelper
    {
        public const string DefaultFileHash = "---";

        public static string ComputeFileHash(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 10 * 1024 * 1024);
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(fs);

                return BitConverter.ToString(hash).ToLower().Replace("-", "");
            }
            catch (Exception)
            {
                return DefaultFileHash;
            }
        }
    }
}
