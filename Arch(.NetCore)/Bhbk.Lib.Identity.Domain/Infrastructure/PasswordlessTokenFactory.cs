using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;

//https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public static class IdentityPasswordlessTokenExtensions
    {
        public static IdentityBuilder AddPasswordlessTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var providerType = typeof(PasswordlessTokenFactory).MakeGenericType(userType);

            return builder.AddTokenProvider(typeof(PasswordlessTokenFactory).Name, providerType);
        }
    }

    public class PasswordlessTokenFactory
    {
        private IDataProtectionProvider _provider;

        public PasswordlessTokenFactory(string appName)
        {
            _provider = DataProtectionProvider.Create(appName);
        }

        public string Generate(string purpose, TimeSpan expire, tbl_Issuers issuer)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", issuer.Id.ToString(), issuer.IssuerKey, purpose);
            var ciphertext = create.Protect(secret, expire);

            return ciphertext;
        }

        public string Generate(string purpose, TimeSpan expire, tbl_Users user)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", user.Id.ToString(), user.SecurityStamp, purpose);
            var ciphertext = create.Protect(secret, expire);

            return ciphertext;
        }

        public bool Validate(string purpose, string token, tbl_Issuers issuer)
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
                return false;
            }

            if (plaintext == secret)
                return true;

            return false;
        }

        public bool Validate(string purpose, string token, tbl_Users user)
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
                return false;
            }

            if (plaintext == secret)
                return true;

            return false;
        }
    }
}
