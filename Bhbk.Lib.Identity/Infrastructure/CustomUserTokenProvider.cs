using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
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
    public class CustomUserTokenProvider : TotpSecurityStampBasedTokenProvider<AppUser>
    {
        public int OtpTokenTimespan;
        public int OtpTokenSize;
        
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<AppUser> manager, AppUser user)
        {
            throw new NotImplementedException();
        }

        public override Task<string> GenerateAsync(string purpose, UserManager<AppUser> manager, AppUser user)
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

        public override Task<bool> ValidateAsync(string purpose, string token, UserManager<AppUser> manager, AppUser user)
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
