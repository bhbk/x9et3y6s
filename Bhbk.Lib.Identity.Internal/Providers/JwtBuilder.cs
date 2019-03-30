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
            CreateAccessTokenV1Legacy(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var issueDate = DateTime.UtcNow;
            var expireDate = DateTime.UtcNow.AddSeconds(86400);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
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
                    claims: principal.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV1(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var issueDate = DateTime.UtcNow;
            var expireDate = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsAccessToken)
            {
                issueDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow;
                expireDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.ClientRepo.GenerateAccessTokenAsync(client);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var issueDate = DateTime.UtcNow;
            var expireDate = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsAccessToken)
            {
                issueDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow;
                expireDate = uow.ConfigRepo.UnitTestsAccessTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            CreateAccessTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, List<TClients> clients, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var issueDate = DateTime.UtcNow;
            var expireDate = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsAccessTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
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
                    claims: principal.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            return (result, issueDate, expireDate);
        }

        public static async Task<string>
            CreateRefreshTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.ClientRepo.GenerateRefreshTokenAsync(client);

            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ExecutionType.UnitTest
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
                    claims: principal.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var refresh = await uow.ClientRepo.CreateRefreshAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    ProtectedTicket = result,
                    IssuedUtc = issueDate,
                    ExpiresUtc = expireDate
                });

            if (refresh == null)
                throw new InvalidOperationException();

            return result;
        }

        public static async Task<string>
            CreateRefreshTokenV1(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateRefreshTokenAsync(user);

            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ExecutionType.UnitTest
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
                    claims: principal.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            var refresh = await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    ProtectedTicket = result,
                    IssuedUtc = issueDate,
                    ExpiresUtc = expireDate
                });

            if(refresh == null)
                throw new InvalidOperationException();

            return result;
        }

        public static async Task<string>
            CreateRefreshTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateRefreshTokenAsync(user);

            DateTime issueDate, expireDate;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ExecutionType.UnitTest
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
                    claims: principal.Claims,
                    notBefore: issueDate,
                    expires: expireDate,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var refresh = await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    ProtectedTicket = result,
                    IssuedUtc = issueDate,
                    ExpiresUtc = expireDate
                });

            if (refresh == null)
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
