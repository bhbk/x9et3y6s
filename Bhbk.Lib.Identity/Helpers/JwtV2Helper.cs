using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Helpers
{
    public class JwtV2Helper
    {
        public static async Task<(string token, DateTime begin, DateTime end)>
            GenerateAccessToken(HttpContext context, AppClient client, List<AppAudience> audiences, AppUser user)
        {
            var ioc = context.RequestServices.GetRequiredService<IIdentityContext>();
            var identity = await ioc.UserMgmt.ClaimProvider.CreateAsync(user);
            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = client.ClientKey;
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

            string audienceList = string.Empty;

            if (audiences.Count == 0)
                throw new InvalidOperationException();

            else
                audienceList = string.Join(",", audiences.Select(x => x.Name.ToString()));

            var access = new JwtSecurityToken(
                issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                audience: audienceList,
                claims: identity.Claims,
                notBefore: issueDate,
                expires: expireDate,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                );

            var result = new JwtSecurityTokenHandler().WriteToken(access);

            return (result, issueDate, expireDate);
        }

        public static async Task<string>
            GenerateRefreshToken(HttpContext context, AppClient client, AppUser user)
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
