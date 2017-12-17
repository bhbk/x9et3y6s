using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Helpers
{
    public class JwtV2Helper
    {
        public static async Task<(string token, DateTime begin, DateTime end)>
            GenerateAccessToken(HttpContext context, AppClient client, AppAudience audience, AppUser user)
        {
            var ioc = context.RequestServices.GetRequiredService<IIdentityContext>();
            DateTime issueDate, expireDate;

            ClaimsIdentity claims = await ioc.UserMgmt.CreateIdentityAsync(user, "JWT");

            ClaimsPrincipal identity = new ClaimsPrincipal();
            identity.AddIdentity(claims);

            var symmetricKeyAsBase64 = audience.AudienceKey;
            var keyBytes = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.ContextStatus == ContextType.UnitTest && ioc.ConfigMgmt.Tweaks.UnitTestAccessToken)
            {
                issueDate = ioc.ConfigMgmt.Tweaks.UnitTestAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Tweaks.UnitTestAccessTokenFakeUtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultAccessTokenLife);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultAccessTokenLife);
            }

            var access = new JwtSecurityToken(
                issuer: client.Id.ToString(),
                audience: audience.Id.ToString(),
                claims: identity.Claims,
                notBefore: issueDate,
                expires: expireDate,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                );

            var result = new JwtSecurityTokenHandler().WriteToken(access);

            return (result, issueDate, expireDate);
        }

        public static async Task<string>
            GenerateRefreshToken(HttpContext context, AppClient client, AppAudience audience, AppUser user)
        {
            var ioc = context.RequestServices.GetRequiredService<IIdentityContext>();
            DateTime issueDate, expireDate;

            if (ioc.ContextStatus == ContextType.UnitTest && ioc.ConfigMgmt.Tweaks.UnitTestRefreshToken)
            {
                issueDate = ioc.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultRefreshTokenLife);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultRefreshTokenLife);
            }

            var refresh = new UserRefreshCreate()
            {
                ClientId = client.Id,
                AudienceId = audience.Id,
                UserId = user.Id,
                ProtectedTicket = CryptoHelper.GenerateRandomBase64(256),
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var result = await ioc.UserMgmt.AddRefreshTokenAsync(new UserRefreshFactory<UserRefreshCreate>(refresh).Devolve());

            if (!result.Succeeded)
                throw new InvalidOperationException();

            return refresh.ProtectedTicket;
        }

        public static bool IsValidJwtFormat(string token)
        {
            //check if string is in jwt format.
            var jwt = new JwtSecurityTokenHandler();

            if (jwt.CanReadToken(token))
                return true;
            else
                return false;
        }
    }
}
