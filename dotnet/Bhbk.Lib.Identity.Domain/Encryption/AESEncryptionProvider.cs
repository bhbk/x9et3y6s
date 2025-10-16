using Bhbk.Lib.Identity.Domain.Factories;
using System;

namespace Bhbk.Lib.Identity.Domain.Encryption
{
    public class AESEncryptionProvider : IConfigEncryptionProvider
    {
        public const string AesPrefix = "AES:";
        private readonly string _key;

        public string Prefix => AesPrefix;

        public AESEncryptionProvider(string base64Key)
        {
            _key = base64Key ?? throw new ArgumentNullException(nameof(base64Key));
        }

        public string Encrypt(string plaintext)
            => AesPrefix + AESEncryptionFactory.Encrypt(plaintext, _key);

        public string Decrypt(string ciphertext)
            => AESEncryptionFactory.Decrypt(ciphertext, _key);

        public bool CanDecrypt(string prefixedValue)
            => !string.IsNullOrEmpty(prefixedValue) && prefixedValue.StartsWith(AesPrefix);
    }
}
