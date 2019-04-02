using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
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
            ClientAccessTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.ClientRepo.GenerateAccessTokenAsync(client);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTime.UtcNow;
            var validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpireClientToken);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsPasswordToken)
            {
                validFromUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpireClientToken);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            return (result, validFromUtc, validToUtc);
        }

        public static async Task<string>
            ClientRefreshTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.ClientRepo.GenerateRefreshTokenAsync(client);

            DateTime validFromUtc, validToUtc;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsPasswordRefresh)
            {
                validFromUtc = uow.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow;
                validToUtc = uow.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpireClientRefresh);
            }
            else
            {
                validFromUtc = DateTime.UtcNow;
                validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpireClientRefresh);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: null,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var refresh = await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    RefreshType = RefreshType.Client.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc
                });

            if (refresh == null)
                throw new InvalidOperationException();

            return result;
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            UserAccessTokenV1Legacy(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTime.UtcNow;
            var validToUtc = DateTime.UtcNow.AddSeconds(86400);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsPasswordToken)
            {
                validFromUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow.AddSeconds(86400);
            }

            //do not use issuer salt for compatibility here...
            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString(),
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return (result, validFromUtc, validToUtc);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            UserAccessTokenV1(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TClients client, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTime.UtcNow;
            var validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordToken);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsPasswordToken)
            {
                validFromUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordToken);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return (result, validFromUtc, validToUtc);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            UserAccessTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, List<TClients> clients, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTime.UtcNow;
            var validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordToken);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsPasswordToken)
            {
                validFromUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.UnitTestsPasswordTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordToken);
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
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            return (result, validFromUtc, validToUtc);
        }

        public static async Task<string>
            UserRefreshTokenV1(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateRefreshTokenAsync(user);

            DateTime validFromUtc, validToUtc;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsPasswordRefresh)
            {
                validFromUtc = uow.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow;
                validToUtc = uow.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordRefresh);
            }
            else
            {
                validFromUtc = DateTime.UtcNow;
                validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordRefresh);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: null,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            var refresh = await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc,
                });

            if (refresh == null)
                throw new InvalidOperationException();

            return result;
        }

        public static async Task<string>
            UserRefreshTokenV2(IIdentityContext<DatabaseContext> uow, TIssuers issuer, TUsers user)
        {
            if (uow == null)
                throw new ArgumentNullException();

            var principal = await uow.UserRepo.GenerateRefreshTokenAsync(user);

            DateTime validFromUtc, validToUtc;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (uow.Situation == ExecutionType.UnitTest
                && uow.ConfigRepo.UnitTestsPasswordRefresh)
            {
                validFromUtc = uow.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow;
                validToUtc = uow.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordRefresh);
            }
            else
            {
                validFromUtc = DateTime.UtcNow;
                validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DefaultsExpirePasswordRefresh);
            }

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.IssuerRepo.Salt,
                    audience: null,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            var refresh = await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc
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
