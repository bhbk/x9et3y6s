using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.AspNetCore.Identity;
using OtpNet;
using System.Text;

//https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

namespace Bhbk.Lib.Identity.Domain.Helpers
{
    public static class IdentityTimeBasedTokenExtensions
    {
        public static IdentityBuilder AddTimeBasedTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var providerType = typeof(TimeBasedTokenFactory).MakeGenericType(userType);

            return builder.AddTokenProvider(typeof(TimeBasedTokenFactory).Name, providerType);
        }
    }

    public class TimeBasedTokenFactory
    {
        private readonly int _length;
        private readonly int _expire;

        public TimeBasedTokenFactory(int length, int expire)
        {
            _length = length;
            _expire = expire;
        }

        public string Generate(string purpose, tbl_Users user)
        {
            byte[] secret = Encoding.Unicode.GetBytes(user.Id.ToString() + purpose);
            Totp code = new Totp(secret, step: _expire, mode: OtpHashMode.Sha512, totpSize: _length);

            return code.ComputeTotp();
        }

        public bool Validate(string purpose, string token, tbl_Users user)
        {
            byte[] secret = Encoding.Unicode.GetBytes(user.Id.ToString() + purpose);
            Totp code = new Totp(secret, step: _expire, mode: OtpHashMode.Sha512, totpSize: _length);

            long timeStep;
            bool result = code.VerifyTotp(token, out timeStep);

            return result;
        }
    }
}
