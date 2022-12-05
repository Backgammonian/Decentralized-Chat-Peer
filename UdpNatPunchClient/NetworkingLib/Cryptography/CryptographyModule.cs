using System;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetworkingLib.Extensions;

namespace NetworkingLib.Cryptography
{
    public sealed class CryptographyModule : IDisposable
    {
        #region Static methods
        public static string CalculateHash(string input)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        public static string CalculateHash(byte[] input)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(input);
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
        #endregion

        private readonly ECDiffieHellman _ecdh;
        private readonly byte[] _publicKey;
        private byte[] _privateKey;
        private readonly ECDsa _ecdsa;
        private readonly byte[] _signaturePublicKey;
        private ECDsa? _ecdsaRecepient;
        private bool _isDisposed;

        public CryptographyModule()
        {
            _ecdh = ECDiffieHellman.Create();
            _publicKey = _ecdh.ExportSubjectPublicKeyInfo();
            _privateKey = Array.Empty<byte>();
            _ecdsa = ECDsa.Create();
            _signaturePublicKey = _ecdsa.ExportSubjectPublicKeyInfo();
            MyPublicKeyHash = CalculateHash(_publicKey.Add(_signaturePublicKey));
            IsEnabled = false;
        }

        public byte[] PublicKey => (byte[])_publicKey.Clone();
        public byte[] SignaturePublicKey => (byte[])_signaturePublicKey.Clone();
        public bool IsEnabled { get; private set; }
        public string MyPublicKeyHash { get; }
        public string RecepientPublicKeyHash { get; private set; } = string.Empty;

        public bool TrySetKeys(byte[] publicKey, byte[] publicSignatureKey)
        {
            if (IsEnabled)
            {
                return false;
            }

            try
            {
                using var ecdhRecepient = ECDiffieHellman.Create();
                ecdhRecepient.ImportSubjectPublicKeyInfo(publicKey, out _);
                _privateKey = _ecdh.DeriveKeyMaterial(ecdhRecepient.PublicKey);

                _ecdsaRecepient = ECDsa.Create();
                _ecdsaRecepient.ImportSubjectPublicKeyInfo(publicSignatureKey, out _);

                RecepientPublicKeyHash = CalculateHash(publicKey.Add(publicSignatureKey));

                IsEnabled = true;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"(TrySetKey) Failure: {ex}");

                return false;
            }
        }

        public bool TrySignData(byte[] data, out byte[] signature)
        {
            signature = Array.Empty<byte>();

            if (!IsEnabled)
            {
                return false;
            }

            try
            {
                signature = _ecdsa.SignData(data, HashAlgorithmName.SHA512);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryVerifySignature(byte[] data, byte[] signature)
        {
            if (!IsEnabled)
            {
                return false;
            }

            if (_ecdsaRecepient == null)
            {
                return false;
            }

            try
            {
                return _ecdsaRecepient.VerifyData(data, signature, HashAlgorithmName.SHA512);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryEncrypt(byte[] secretMessage, out byte[] encryptedMessage, out byte[] iv)
        {
            iv = Array.Empty<byte>();
            encryptedMessage = Array.Empty<byte>();

            if (!IsEnabled)
            {
                return false;
            }

            try
            {
                using Aes aes = new AesCryptoServiceProvider();
                aes.Key = _privateKey;
                iv = aes.IV;
                using var cipherText = new MemoryStream();
                using var cryptoStream = new CryptoStream(cipherText, aes.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(secretMessage, 0, secretMessage.Length);
                cryptoStream.Close();
                encryptedMessage = cipherText.ToArray();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryDecrypt(byte[] encryptedMessage, byte[] iv, out byte[] decryptedMessage)
        {
            decryptedMessage = Array.Empty<byte>();

            if (!IsEnabled)
            {
                return false;
            }

            try
            {
                using Aes aes = new AesCryptoServiceProvider();
                aes.Key = _privateKey;
                aes.IV = iv;
                using var decryptionStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(decryptionStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(encryptedMessage, 0, encryptedMessage.Length);
                cryptoStream.Close();
                decryptedMessage = decryptionStream.ToArray();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    IsEnabled = false;

                    _ecdh.Dispose();
                    _ecdsa.Dispose();
                    _ecdsaRecepient?.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}