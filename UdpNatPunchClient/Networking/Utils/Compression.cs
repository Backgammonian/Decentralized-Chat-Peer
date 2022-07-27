using System;
using System.IO;
using System.IO.Compression;

namespace Networking.Utils
{
    public static class Compression
    {
        public static bool TryCompressByteArray(this byte[] data, out byte[] result)
        {
            try
            {
                using var compressedStream = new MemoryStream();
                using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                result = compressedStream.ToArray();

                return true;
            }
            catch (Exception)
            {
                result = Array.Empty<byte>();

                return false;
            }
        }

        public static bool TryDecompressByteArray(this byte[] data, out byte[] result)
        {
            try
            {
                using var compressedStream = new MemoryStream(data);
                using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                using var resultStream = new MemoryStream();
                zipStream.CopyTo(resultStream);
                result = resultStream.ToArray();

                return true;
            }
            catch (Exception)
            {
                result = Array.Empty<byte>();

                return false;
            }
        }
    }
}
