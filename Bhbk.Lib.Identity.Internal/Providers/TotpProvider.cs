using Bhbk.Lib.Identity.Internal.EntityModels;
using Microsoft.AspNetCore.Identity;
using OtpNet;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Providers
{
    public class TotpProvider
    {
        private readonly int _length;
        private readonly int _expire;

        public TotpProvider(int length, int expire)
        {
            _length = length;
            _expire = expire;
        }

        public Task<string> GenerateAsync(string purpose, TUsers user)
        {
            byte[] secret = Encoding.Unicode.GetBytes(user.Id.ToString() + purpose);
            Totp code = new Totp(secret, step: _expire, mode: OtpHashMode.Sha512, totpSize: _length);

            return Task.FromResult<string>(code.ComputeTotp());
        }

        public Task<bool> ValidateAsync(string purpose, string token, TUsers user)
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
        public static IdentityBuilder AddMyTotpProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var providerType = typeof(TotpProvider).MakeGenericType(userType);

            return builder.AddTokenProvider(typeof(TotpProvider).Name, providerType);
        }
    }
}
