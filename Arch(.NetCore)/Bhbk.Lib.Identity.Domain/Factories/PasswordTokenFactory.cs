using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;

//https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public static class IdentityPasswordlessTokenExtensions
    {
        public static IdentityBuilder AddPasswordlessTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var providerType = typeof(PasswordTokenFactory).MakeGenericType(userType);

            return builder.AddTokenProvider(typeof(PasswordTokenFactory).Name, providerType);
        }
    }

    public class PasswordTokenFactory
    {
        private IDataProtectionProvider _provider;

        public PasswordTokenFactory(string appName)
        {
            _provider = DataProtectionProvider.Create(appName);
        }

        public string Generate(string purpose, TimeSpan expire, tbl_Issuer issuer)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", issuer.Id.ToString(), issuer.IssuerKey, purpose);
            var ciphertext = create.Protect(secret, expire);

            return ciphertext;
        }

        public string Generate(string purpose, TimeSpan expire, tbl_User user)
        {
            var create = _provider.CreateProtector(purpose).ToTimeLimitedDataProtector();
            var secret = string.Format("{0}/{1}/{2}", user.Id.ToString(), user.SecurityStamp, purpose);
            var ciphertext = create.Protect(secret, expire);

            return ciphertext;
        }

        public bool Validate(string purpose, string token, tbl_Issuer issuer)
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

        public bool Validate(string purpose, string token, tbl_User user)
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
