using System;

namespace NetworkingLib.Extensions
{
    internal static class ByteArrayExtensions
    {
        public static byte[] Add(this byte[] source, byte[] value)
        {
            if (source == null ||
                value == null)
            {
                return Array.Empty<byte>();
            }

            var result = new byte[source.Length + value.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = source[i];
            }
            for (int i = source.Length; i < source.Length + value.Length; i++)
            {
                result[i] = value[i - source.Length];
            }

            return result;
        }
    }
}
