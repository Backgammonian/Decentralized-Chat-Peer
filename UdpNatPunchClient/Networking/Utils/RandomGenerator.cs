using System;
using System.Text;
using System.Security.Cryptography;

namespace Networking.Utils
{
    public sealed class RandomGenerator : IDisposable
    {
        private const string _chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private readonly static Random _random = new Random();

        private readonly RNGCryptoServiceProvider _csp;
        private bool _isDisposed;

        public RandomGenerator()
        {
            _csp = new RNGCryptoServiceProvider();
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));

            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            var buffer = new byte[bytesNumber];
            _csp.GetBytes(buffer);

            return buffer;
        }

        public int Next(int minValue, int maxExclusiveValue)
        {
            if (minValue == maxExclusiveValue)
            {
                return minValue;
            }

            if (minValue > maxExclusiveValue)
            {
                var t = minValue;
                minValue = maxExclusiveValue;
                maxExclusiveValue = t;
            }

            var diff = (long)maxExclusiveValue - minValue;
            var upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            }
            while (ui >= upperBound);

            return (int)(minValue + (ui % diff));
        }

        public static string GetRandomString(int length)
        {
            using var rnd = new RandomGenerator();

            var result = new StringBuilder();
            for (var j = 0; j < length; j++)
            {
                result.Append(_chars[rnd.Next(0, _chars.Length - 1)]);
            }

            return result.ToString();
        }

        public static long GetRandomLong()
        {
            using var rnd = new RandomGenerator();
            var result = "";
            for (var j = 0; j < 64; j++)
            {
                result += rnd.Next(0, 100) < 50 ? "0" : "1";
            }

            return Convert.ToInt64(result, 2);
        }

        public static ulong GetRandomULong()
        {
            using var rnd = new RandomGenerator();
            var result = "";
            for (var j = 0; j < 64; j++)
            {
                result += rnd.Next(0, 100) < 50 ? "0" : "1";
            }

            return Convert.ToUInt64(result, 2);
        }

        public static byte GetPseudoRandomByte(byte minValue, byte maxValue)
        {
            return (byte)_random.Next(minValue, maxValue);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_csp != null)
            {
                _csp.Dispose();
            }

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
