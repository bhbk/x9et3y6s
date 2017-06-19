using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using OtpNet;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://www.stevejgordon.co.uk/asp-net-core-identity-token-providers
    //https://blog.kraken.com/post/291/the-importance-of-two-factor-authentication/
    //https://github.com/kspearrin/Otp.NET

    //https://tools.ietf.org/html/rfc6238
    public class CustomUserTokenProvider : TotpSecurityStampBasedTokenProvider<AppUser, Guid>, IUserTokenProvider<AppUser, Guid>
    {
        public int OtpTokenTimespan;
        public int OtpTokenSize;

        public override Task<string> GenerateAsync(string purpose, UserManager<AppUser, Guid> manager, AppUser user)
        {
            if (manager == null)
                throw new ArgumentNullException();

            if (purpose == Statics.ApiTokenConfirmEmail || purpose == Statics.ApiTokenResetPassword)
            {
                return Task.FromResult<string>(user.SecurityStamp);
            }
            else if (purpose == Statics.ApiTokenConfirmPhone || purpose == Statics.ApiTokenConfirmTwoFactor)
            {
                byte[] secret = Encoding.Unicode.GetBytes(user.SecurityStamp + purpose);
                Totp code = new Totp(secret, step: OtpTokenTimespan, mode: OtpHashMode.Sha1, totpSize: OtpTokenSize);

                return Task.FromResult<string>(code.ComputeTotp());
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<bool> IsValidProviderForUserAsync(UserManager<AppUser, Guid> manager, AppUser user)
        {
            if (manager == null)
                throw new ArgumentNullException();

            return Task.FromResult<bool>(manager.SupportsUserPassword);
        }

        public override Task NotifyAsync(string token, UserManager<AppUser, Guid> manager, AppUser user)
        {
            if (manager == null)
                throw new ArgumentNullException();

            return Task.FromResult<int>(0);
        }

        public override Task<bool> ValidateAsync(string purpose, string token, UserManager<AppUser, Guid> manager, AppUser user)
        {
            if (manager == null)
                throw new ArgumentNullException();

            if (purpose == Statics.ApiTokenConfirmEmail || purpose == Statics.ApiTokenResetPassword)
            {
                if (token == user.SecurityStamp)
                    return Task.FromResult(true);
                else
                    return Task.FromResult(false);
            }
            else if (purpose == Statics.ApiTokenConfirmPhone || purpose == Statics.ApiTokenConfirmTwoFactor)
            {
                byte[] secret = Encoding.Unicode.GetBytes(user.SecurityStamp + purpose);
                Totp code = new Totp(secret, step: OtpTokenTimespan, mode: OtpHashMode.Sha1, totpSize: OtpTokenSize);

                long timeStep;
                bool result = code.VerifyTotp(token, out timeStep);

                return Task.FromResult(result);
            }
            else
                throw new ArgumentNullException();
        }
    }
}
