using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography;

//https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public class PasswordTokenFactory
    {
        private IDataProtectionProvider _provider;

        public PasswordTokenFactory(string appName)
        {
            _provider = DataProtectionProvider.Create(appName);
        }

        public string Generate(string purpose, TimeSpan expire, string entity, string session)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", purpose, entity, session);
            var ciphertext = create.Protect(secret, expire);

            return ciphertext;
        }

        public bool Validate(string purpose, string token, string entity, string session)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", purpose, entity, session);
            string plaintext = string.Empty;

            try
            {
                DateTimeOffset time;
                plaintext = create.Unprotect(token, out time);
            }
            catch (CryptographicException)
            {
                return false;
            }

            if (plaintext == secret)
                return true;

            return false;
        }
    }
}
