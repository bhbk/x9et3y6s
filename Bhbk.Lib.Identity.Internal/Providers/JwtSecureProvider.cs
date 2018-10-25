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
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.Status == ContextType.UnitTest
                && ioc.ConfigStore.Values.UnitTestsAccessToken)
            {
                issueDate = ioc.ConfigStore.Values.UnitTestsAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigStore.Values.UnitTestsAccessTokenFakeUtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsAccessTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsAccessTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                    audience: audience.Name.ToString(),
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV1CompatibilityMode(IIdentityContext ioc, AppClient client, AppAudience audience, AppUser user)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            var identity = await ioc.UserMgmt.ClaimProvider.CreateAsync(user);

            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = client.ClientKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.Status == ContextType.UnitTest
                && ioc.ConfigStore.Values.UnitTestsAccessToken)
            {
                issueDate = ioc.ConfigStore.Values.UnitTestsAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigStore.Values.UnitTestsAccessTokenFakeUtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsAccessTokenExpire);
            }
            else
            {
                //use hard coded value for expire...
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(86400);
            }

            //do not use issuer salt for compatibility here...
            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: client.Name.ToString(),
                    audience: audience.Name.ToString(),
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

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
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.Status == ContextType.UnitTest && ioc.ConfigStore.Values.UnitTestsAccessToken)
            {
                issueDate = ioc.ConfigStore.Values.UnitTestsAccessTokenFakeUtcNow;
                expireDate = ioc.ConfigStore.Values.UnitTestsAccessTokenFakeUtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsAccessTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsAccessTokenExpire);
            }

            string audienceList = string.Empty;

            if (audiences.Count == 0)
                throw new InvalidOperationException();

            else
                audienceList = string.Join(", ", audiences.Select(x => x.Name.ToString()).OrderBy(x => x));

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                    audience: audienceList,
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

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
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.Status == ContextType.UnitTest && ioc.ConfigStore.Values.UnitTestsRefreshToken)
            {
                issueDate = ioc.ConfigStore.Values.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = ioc.ConfigStore.Values.UnitTestsRefreshTokenFakeUtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsRefreshTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsRefreshTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                    audience: null,
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var create = new UserRefreshCreate()
            {
                ClientId = client.Id,
                UserId = user.Id,
                ProtectedTicket = result,
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var refresh = await ioc.UserMgmt.AddRefreshTokenAsync(new UserRefreshFactory<UserRefreshCreate>(create).Devolve());

            if (!refresh.Succeeded)
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
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (ioc.Status == ContextType.UnitTest && ioc.ConfigStore.Values.UnitTestsRefreshToken)
            {
                issueDate = ioc.ConfigStore.Values.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = ioc.ConfigStore.Values.UnitTestsRefreshTokenFakeUtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsRefreshTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(ioc.ConfigStore.Values.DefaultsRefreshTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: client.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                    audience: null,
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var create = new UserRefreshCreate()
            {
                ClientId = client.Id,
                UserId = user.Id,
                ProtectedTicket = result,
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var refresh = await ioc.UserMgmt.AddRefreshTokenAsync(new UserRefreshFactory<UserRefreshCreate>(create).Devolve());

            if (!refresh.Succeeded)
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
