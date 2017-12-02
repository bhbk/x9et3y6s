using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
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
    public class JwtHelperV2
    {
        public static async Task<(string token, DateTime begin, DateTime end)>
            GenerateAccessToken(HttpContext context, ClientModel client, AudienceModel audience, UserModel user)
        {
            var ioc = context.RequestServices.GetService<IIdentityContext>();
            DateTime issueDate, expireDate;

            ClaimsIdentity claims = ioc.UserMgmt.CreateIdentityAsync(ioc.UserMgmt.Store.Mf.Devolve.DoIt(user), "JWT").Result;

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
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            var result = new JwtSecurityTokenHandler().WriteToken(access);

            return (result, issueDate, expireDate);
        }

        public static async Task<string>
            GenerateRefreshToken(HttpContext context, ClientModel client, AudienceModel audience, UserModel user)
        {
            var ioc = context.RequestServices.GetService<IIdentityContext>();
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

            var remove = ioc.UserMgmt.RemoveRefreshTokenAsync(client.Id, audience.Id, user.Id).Result;

            if (!remove.Succeeded)
                throw new InvalidOperationException();

            var result = new UserRefreshCreate()
            {
                ClientId = client.Id,
                AudienceId = audience.Id,
                UserId = user.Id,
                ProtectedTicket = EntrophyHelper.GenerateRandomBase64(256),
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var create = ioc.UserMgmt.Store.Mf.Create.DoIt(result);
            var add = ioc.UserMgmt.AddRefreshTokenAsync(create).Result;

            if (!add.Succeeded)
                throw new InvalidOperationException();

            return result.ProtectedTicket;
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
