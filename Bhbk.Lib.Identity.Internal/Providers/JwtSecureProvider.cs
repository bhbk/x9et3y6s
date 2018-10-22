using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class JwtSecureProvider
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

            if (ioc.Status == ContextType.UnitTest && ioc.ConfigMgmt.Store.UnitTestsAccessToken)
            {
                issueDate = ioc.ConfigMgmt.Store.UnitTestsAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Store.UnitTestsAccessTokenFakeUtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsAccessTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsAccessTokenExpire);
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

            if (ioc.Status == ContextType.UnitTest && ioc.ConfigMgmt.Store.UnitTestsAccessToken)
            {
                issueDate = ioc.ConfigMgmt.Store.UnitTestsAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Store.UnitTestsAccessTokenFakeUtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsAccessTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsAccessTokenExpire);
            }

            string audienceList = string.Empty;

            if (audiences.Count == 0)
                throw new InvalidOperationException();

            else
                audienceList = string.Join(", ", audiences.Select(x => x.Name.ToString()).OrderBy(x => x));

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

            if (ioc.Status == ContextType.UnitTest && ioc.ConfigMgmt.Store.UnitTestsRefreshToken)
            {
                issueDate = ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsRefreshTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsRefreshTokenExpire);
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
            CreateRefreshTokenV2(IIdentityContext ioc, AppClient client, AppUser user)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            var identity = await ioc.UserMgmt.ClaimProvider.CreateRefreshAsync(user);
            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = client.ClientKey;
            var keyBytes = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.Status == ContextType.UnitTest && ioc.ConfigMgmt.Store.UnitTestsRefreshToken)
            {
                issueDate = ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsRefreshTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigMgmt.Store.DefaultsRefreshTokenExpire);
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
    }
}
