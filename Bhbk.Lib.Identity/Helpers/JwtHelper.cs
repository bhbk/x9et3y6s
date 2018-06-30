using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Helpers
{
    public class JwtHelper
    {
        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV1(IIdentityContext ioc, AppClient client, AppAudience audience, AppUser user)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            var identity = await ioc.UserMgmt.ClaimProvider.CreateAsync(user);
            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = client.ClientKey;
            var keyBytes = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.ContextStatus == ContextType.UnitTest && ioc.ConfigMgmt.Tweaks.UnitTestsAccessToken)
            {
                issueDate = ioc.ConfigMgmt.Tweaks.UnitTestsAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Tweaks.UnitTestsAccessTokenFakeUtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsAccessTokenLife);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsAccessTokenLife);
            }

            var access = new JwtSecurityToken(
                issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                audience: audience.Name.ToString(),
                claims: identity.Claims,
                notBefore: issueDate,
                expires: expireDate,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            var result = new JwtSecurityTokenHandler().WriteToken(access);

            return (result, issueDate, expireDate);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV2(IIdentityContext ioc, AppClient client, List<AppAudience> audiences, AppUser user)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            var identity = await ioc.UserMgmt.ClaimProvider.CreateAsync(user);
            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = client.ClientKey;
            var keyBytes = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.ContextStatus == ContextType.UnitTest && ioc.ConfigMgmt.Tweaks.UnitTestsAccessToken)
            {
                issueDate = ioc.ConfigMgmt.Tweaks.UnitTestsAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Tweaks.UnitTestsAccessTokenFakeUtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsAccessTokenLife);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsAccessTokenLife);
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
            CreateRefreshTokenV1(IIdentityContext ioc, AppClient client, AppUser user)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            var identity = await ioc.UserMgmt.ClaimProvider.CreateRefreshAsync(user);
            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = client.ClientKey;
            var keyBytes = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.ContextStatus == ContextType.UnitTest && ioc.ConfigMgmt.Tweaks.UnitTestsRefreshToken)
            {
                issueDate = ioc.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsRefreshTokenLife);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsRefreshTokenLife);
            }

            var refresh = new JwtSecurityToken(
                issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                audience: null,
                claims: identity.Claims,
                notBefore: issueDate,
                expires: expireDate,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                );

            var result = new JwtSecurityTokenHandler().WriteToken(refresh);

            var create = new UserRefreshCreate()
            {
                ClientId = client.Id,
                UserId = user.Id,
                ProtectedTicket = result,
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var add = await ioc.UserMgmt.AddRefreshTokenAsync(new UserRefreshFactory<UserRefreshCreate>(create).Devolve());

            if (!add.Succeeded)
                throw new InvalidOperationException();

            return result;
        }

        public static async Task<string>
            CreatefreshTokenV2(IIdentityContext ioc, AppClient client, AppUser user)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            var identity = await ioc.UserMgmt.ClaimProvider.CreateRefreshAsync(user);
            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = client.ClientKey;
            var keyBytes = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.ContextStatus == ContextType.UnitTest && ioc.ConfigMgmt.Tweaks.UnitTestsRefreshToken)
            {
                issueDate = ioc.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsRefreshTokenLife);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddMinutes(ioc.ConfigMgmt.Tweaks.DefaultsRefreshTokenLife);
            }

            var refresh = new JwtSecurityToken(
                issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                audience: null,
                claims: identity.Claims,
                notBefore: issueDate,
                expires: expireDate,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                );

            var result = new JwtSecurityTokenHandler().WriteToken(refresh);

            var create = new UserRefreshCreate()
            {
                ClientId = client.Id,
                UserId = user.Id,
                ProtectedTicket = result,
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var add = await ioc.UserMgmt.AddRefreshTokenAsync(new UserRefreshFactory<UserRefreshCreate>(create).Devolve());

            if (!add.Succeeded)
                throw new InvalidOperationException();

            return result;
        }

        public static bool IsValidJwtFormat(string str)
        {
            //check if string is in jwt format.
            var jwt = new JwtSecurityTokenHandler();

            if (jwt.CanReadToken(str))
                return true;
            else
                return false;
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            GetAccessTokenV2(IIdentityContext ioc, string clientName, string audienceName, string userName)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            var client = ioc.ClientMgmt.Store.Get(x => x.Name == clientName).Single();
            var audience = ioc.AudienceMgmt.Store.Get(x => x.Name == audienceName).Single();
            var user = ioc.UserMgmt.Store.Get(x => x.Email == userName).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            return CreateAccessTokenV2(ioc, client, audiences, user).Result;
        }
    }
}
