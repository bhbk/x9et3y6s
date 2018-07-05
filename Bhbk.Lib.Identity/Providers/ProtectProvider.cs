using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/overview
//https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/non-di-scenarios

namespace Bhbk.Lib.Identity.Providers
{
    //https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

    public class ProtectProvider
    {
        private IDataProtectionProvider _provider;

        public ProtectProvider(string applicationName)
        {
            _provider = DataProtectionProvider.Create(applicationName);
        }

        public Task<string> GenerateAsync(string purpose, TimeSpan expire, AppClient client)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", client.Id.ToString(), client.ClientKey, purpose);
            var ciphertext = create.Protect(secret, expire);

            return Task.FromResult<string>(ciphertext);
        }

        public Task<string> GenerateAsync(string purpose, TimeSpan expire, AppUser user)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", user.Id.ToString(), user.SecurityStamp, purpose);
            var ciphertext = create.Protect(secret, expire);

            return Task.FromResult<string>(ciphertext);
        }

        public Task<bool> ValidateAsync(string purpose, string token, AppClient client)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", client.Id.ToString(), client.ClientKey, purpose);
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

        public Task<bool> ValidateAsync(string purpose, string token, AppUser user)
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
