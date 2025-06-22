using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public static class AESEncryptionFactory
    {
        private const int KeySize = 32;   /* 256 bits */
        private const int NonceSize = 12;
        private const int TagSize = 16;

        public static string Encrypt(string plaintext, string base64Key)
        {
            var key = Convert.FromBase64String(base64Key);

            if (key.Length != KeySize)
                throw new ArgumentException($"Key must be {KeySize} bytes (256 bits)");

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(
                new KeyParameter(key), TagSize * 8, nonce, null);
            cipher.Init(true, parameters);

            /* BouncyCastle outputs: ciphertext + tag */
            var output = new byte[cipher.GetOutputSize(plaintextBytes.Length)];
            var len = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, output, 0);
            cipher.DoFinal(output, len);

            /* extract tag from end of output */
            var ciphertext = new byte[output.Length - TagSize];
            var tag = new byte[TagSize];
            Buffer.BlockCopy(output, 0, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(output, ciphertext.Length, tag, 0, TagSize);

            /* maintain existing format: nonce + tag + ciphertext */
            var result = new byte[NonceSize + TagSize + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string encryptedBase64, string base64Key)
        {
            var key = Convert.FromBase64String(base64Key);
            var data = Convert.FromBase64String(encryptedBase64);

            /* parse existing format: nonce + tag + ciphertext */
            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            var ciphertext = new byte[data.Length - NonceSize - TagSize];

            Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(data, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(data, NonceSize + TagSize, ciphertext, 0, ciphertext.Length);

            /* BouncyCastle expects: ciphertext + tag */
            var input = new byte[ciphertext.Length + TagSize];
            Buffer.BlockCopy(ciphertext, 0, input, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, input, ciphertext.Length, TagSize);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(
                new KeyParameter(key), TagSize * 8, nonce, null);
            cipher.Init(false, parameters);

            var output = new byte[cipher.GetOutputSize(input.Length)];
            var len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            cipher.DoFinal(output, len);

            return Encoding.UTF8.GetString(output);
        }

        public static string GenerateKey()
        {
            var key = new byte[KeySize];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }
    }
}
