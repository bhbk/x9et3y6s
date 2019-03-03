using Bhbk.Lib.Identity.DomainModels.Admin;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Providers
{
    //https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

    public class ProtectProvider
    {
        private IDataProtectionProvider _provider;

        public ProtectProvider(string applicationName)
        {
            _provider = DataProtectionProvider.Create(applicationName);
        }

        public Task<string> GenerateAsync(string purpose, TimeSpan expire, IssuerModel issuer)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", issuer.Id.ToString(), issuer.IssuerKey, purpose);
            var ciphertext = create.Protect(secret, expire);

            return Task.FromResult<string>(ciphertext);
        }

        public Task<string> GenerateAsync(string purpose, TimeSpan expire, UserModel user)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", user.Id.ToString(), user.SecurityStamp, purpose);
            var ciphertext = create.Protect(secret, expire);

            return Task.FromResult<string>(ciphertext);
        }

        public Task<bool> ValidateAsync(string purpose, string token, IssuerModel issuer)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", issuer.Id.ToString(), issuer.IssuerKey, purpose);
            string plaintext = string.Empty;

            try
            {
                DateTimeOffset time;
                plaintext = create.Unprotect(token, out time);
            }
            catch (CryptographicException)
            {
                return Task.FromResult<bool>(false);
            }

            if (plaintext == secret)
                return Task.FromResult<bool>(true);

            return Task.FromResult<bool>(false);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserModel user)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", user.Id.ToString(), user.SecurityStamp, purpose);
            string plaintext = string.Empty;

            try
            {
                DateTimeOffset time;
                plaintext = create.Unprotect(token, out time);
            }
            catch (CryptographicException)
            {
                return Task.FromResult<bool>(false);
            }

            if (plaintext == secret)
                return Task.FromResult<bool>(true);

            return Task.FromResult<bool>(false);
        }
    }
}
