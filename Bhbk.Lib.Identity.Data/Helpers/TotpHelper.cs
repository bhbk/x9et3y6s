using Bhbk.Lib.Identity.Data.Models;
using Microsoft.AspNetCore.Identity;
using OtpNet;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Helpers
{
    public class TotpHelper
    {
        private readonly int _length;
        private readonly int _expire;

        public TotpHelper(int length, int expire)
        {
            _length = length;
            _expire = expire;
        }

        public Task<string> GenerateAsync(string purpose, tbl_Users user)
        {
            byte[] secret = Encoding.Unicode.GetBytes(user.Id.ToString() + purpose);
            Totp code = new Totp(secret, step: _expire, mode: OtpHashMode.Sha512, totpSize: _length);

            return Task.FromResult<string>(code.ComputeTotp());
        }

        public Task<bool> ValidateAsync(string purpose, string token, tbl_Users user)
        {
            byte[] secret = Encoding.Unicode.GetBytes(user.Id.ToString() + purpose);
            Totp code = new Totp(secret, step: _expire, mode: OtpHashMode.Sha512, totpSize: _length);

            long timeStep;
            bool result = code.VerifyTotp(token, out timeStep);

            return Task.FromResult(result);
        }
    }
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddIdentityTotpProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var providerType = typeof(TotpHelper).MakeGenericType(userType);

            return builder.AddTokenProvider(typeof(TotpHelper).Name, providerType);
        }
    }
}
