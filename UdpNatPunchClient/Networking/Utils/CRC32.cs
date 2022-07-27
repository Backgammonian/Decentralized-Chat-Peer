using System.Text;
using Force.Crc32;

namespace Networking.Utils
{
    public static class CRC32
    {
        public static uint Compute(byte[] array)
        {
            return Crc32CAlgorithm.Compute(array);
        }

        public static uint Compute(string str)
        {
            return Compute(Encoding.UTF8.GetBytes(str));
        }
    }
}
