using Microsoft.AspNetCore.Authentication;
using System;
using System.Security.Cryptography;

namespace Bhbk.Lib.Identity.Helpers
{
    public class EntrophyHelper
    {
        public static string GenerateRandomBase64(int length)
        {
            byte[] byteValue = new byte[length];
            RNGCryptoServiceProvider.Create().GetBytes(byteValue);
            
            return Base64UrlTextEncoder.Encode(byteValue);
        }

        public static string GenerateSHA256(string input)
        {
            HashAlgorithm algo = new SHA256CryptoServiceProvider();
            
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] byteHash = algo.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
