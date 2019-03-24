using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Providers
{
    public class JwtBuilder
    {
        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV1(IIdentityContext<AppDbContext> uow, AppIssuer issuer, AppClient client, AppUser user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var identity = await uow.UserRepo.CreateClaimsAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var issueDate = DateTime.UtcNow;
            var expireDate = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ContextType.UnitTest
                && uow.ConfigRepo.UnitTestsAccessToken)
            {
                issueDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow;
                expireDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: client.Name.ToString(),
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV1CompatibilityMode(IIdentityContext<AppDbContext> uow, AppIssuer issuer, AppClient client, AppUser user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var identity = await uow.UserRepo.CreateClaimsAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var issueDate = DateTime.UtcNow;
            var expireDate = DateTime.UtcNow.AddSeconds(86400);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ContextType.UnitTest
                && uow.ConfigRepo.UnitTestsAccessToken)
            {
                issueDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow;
                expireDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow.AddSeconds(86400);
            }

            //do not use issuer salt for compatibility here...
            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString(),
                    audience: client.Name.ToString(),
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV2(IIdentityContext<AppDbContext> uow, AppIssuer issuer, List<AppClient> clients, AppUser user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var identity = await uow.UserRepo.CreateClaimsAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var issueDate = DateTime.UtcNow;
            var expireDate = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ContextType.UnitTest
                && uow.ConfigRepo.UnitTestsAccessToken)
            {
                issueDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow;
                expireDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);
            }

            string clientList = string.Empty;

            if (clients.Count == 0)
                throw new InvalidOperationException();

            else
                clientList = string.Join(", ", clients.Select(x => x.Name.ToString()).OrderBy(x => x));

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: clientList,
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<string>
            CreateRefreshTokenV1(IIdentityContext<AppDbContext> uow, AppIssuer issuer, AppUser user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var identity = await uow.UserRepo.CreateClaimsRefreshAsync(user);

            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ContextType.UnitTest 
                && uow.ConfigRepo.UnitTestsRefreshToken)
            {
                issueDate = uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsRefreshTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsRefreshTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: null,
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var create = new UserRefreshCreate()
            {
                IssuerId = issuer.Id,
                UserId = user.Id,
                ProtectedTicket = result,
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var refresh = await uow.UserRepo.AddRefreshTokenAsync(uow.Transform.Map<AppUserRefresh>(create));

            if (!refresh.Succeeded)
                throw new InvalidOperationException();

            return result;
        }

        public static async Task<string>
            CreateRefreshTokenV2(IIdentityContext<AppDbContext> uow, AppIssuer issuer, AppUser user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var identity = await uow.UserRepo.CreateClaimsRefreshAsync(user);

            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ContextType.UnitTest 
                && uow.ConfigRepo.UnitTestsRefreshToken)
            {
                issueDate = uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow;
                expireDate = uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsRefreshTokenExpire);
            }
            else
            {
                issueDate = DateTime.UtcNow;
                expireDate = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsRefreshTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: null,
                    claims: identity.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var create = new UserRefreshCreate()
            {
                IssuerId = issuer.Id,
                UserId = user.Id,
                ProtectedTicket = result,
                IssuedUtc = issueDate,
                ExpiresUtc = expireDate
            };

            var refresh = await uow.UserRepo.AddRefreshTokenAsync(uow.Transform.Map<AppUserRefresh>(create));

            if (!refresh.Succeeded)
                throw new InvalidOperationException();

            return result;
        }

        public static bool CanReadToken(string jwt)
        {
            return new JwtSecurityTokenHandler().CanReadToken(jwt);
        }

        public static JwtSecurityToken ReadJwtToken(string jwt)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        }
    }
}
