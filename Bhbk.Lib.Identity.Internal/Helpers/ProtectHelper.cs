using Bhbk.Lib.Identity.Internal.Models;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Helpers
{
    //https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

    public class ProtectHelper
    {
        private IDataProtectionProvider _provider;

        public ProtectHelper(string appName)
        {
            _provider = DataProtectionProvider.Create(appName);
        }

        public Task<string> GenerateAsync(string purpose, TimeSpan expire, tbl_Issuers issuer)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", issuer.Id.ToString(), issuer.IssuerKey, purpose);
            var ciphertext = create.Protect(secret, expire);

            return Task.FromResult<string>(ciphertext);
        }

        public Task<string> GenerateAsync(string purpose, TimeSpan expire, tbl_Users user)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", user.Id.ToString(), user.SecurityStamp, purpose);
            var ciphertext = create.Protect(secret, expire);

            return Task.FromResult<string>(ciphertext);
        }

        public Task<bool> ValidateAsync(string purpose, string token, tbl_Issuers issuer)
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

        public Task<bool> ValidateAsync(string purpose, string token, tbl_Users user)
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
