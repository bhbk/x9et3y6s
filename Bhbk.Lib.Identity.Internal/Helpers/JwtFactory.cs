using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Helpers
{
    public class JwtFactory
    {
        public static async Task<string>
            ClientRefreshV2(IIdentityUnitOfWork uow, tbl_Issuers issuer, tbl_Clients client)
        {
            var principal = await uow.ClientRepo.GenerateRefreshTokenAsync(client);

            DateTime validFromUtc, validToUtc;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.InstanceType == InstanceContext.UnitTest
                && uow.ConfigRepo.ResourceOwnerRefreshFake)
            {
                validFromUtc = uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow;
                validToUtc = uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow.AddSeconds(uow.ConfigRepo.ClientCredRefreshExpire);
            }
            else
            {
                validFromUtc = DateTime.UtcNow;
                validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.ClientCredRefreshExpire);
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

            await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    RefreshType = RefreshType.Client.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc
                });

            await uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    ClientId = client.Id,
                    ActivityType = LoginType.CreateClientRefreshTokenV2.ToString(),
                    Immutable = false
                });

            await uow.CommitAsync();

            return result;
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            ClientResourceOwnerV2(IIdentityUnitOfWork uow, tbl_Issuers issuer, tbl_Clients client)
        {
            var principal = await uow.ClientRepo.GenerateAccessTokenAsync(client);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTime.UtcNow;
            var validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.ClientCredTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.InstanceType == InstanceContext.UnitTest
                && uow.ConfigRepo.ResourceOwnerTokenFake)
            {
                validFromUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.ClientCredTokenExpire);
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

            await uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    ClientId = client.Id,
                    ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                    Immutable = false
                });

            await uow.CommitAsync();

            return (result, validFromUtc, validToUtc);
        }

        public static async Task<string>
            UserRefreshV1(IIdentityUnitOfWork uow, tbl_Issuers issuer, tbl_Users user)
        {
            var principal = await uow.UserRepo.GenerateRefreshTokenAsync(user);

            DateTime validFromUtc, validToUtc;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.InstanceType == InstanceContext.UnitTest
                && uow.ConfigRepo.ResourceOwnerRefreshFake)
            {
                validFromUtc = uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow;
                validToUtc = uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerRefreshExpire);
            }
            else
            {
                validFromUtc = DateTime.UtcNow;
                validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerRefreshExpire);
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

            await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc,
                });

            await uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV1.ToString(),
                    Immutable = false
                });

            await uow.CommitAsync();

            return result;
        }

        public static async Task<string>
            UserRefreshV2(IIdentityUnitOfWork uow, tbl_Issuers issuer, tbl_Users user)
        {
            var principal = await uow.UserRepo.GenerateRefreshTokenAsync(user);

            DateTime validFromUtc, validToUtc;

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.InstanceType == InstanceContext.UnitTest
                && uow.ConfigRepo.ResourceOwnerRefreshFake)
            {
                validFromUtc = uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow;
                validToUtc = uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerRefreshExpire);
            }
            else
            {
                validFromUtc = DateTime.UtcNow;
                validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerRefreshExpire);
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

            await uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc
                });

            await uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV2.ToString(),
                    Immutable = false
                });

            await uow.CommitAsync();

            return result;
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            UserResourceOwnerV1_Legacy(IIdentityUnitOfWork uow, tbl_Issuers issuer, tbl_Clients client, tbl_Users user)
        {
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

            if (uow.InstanceType == InstanceContext.UnitTest
                && uow.ConfigRepo.ResourceOwnerTokenFake)
            {
                validFromUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow.AddSeconds(86400);
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

            await uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV1Legacy.ToString(),
                    Immutable = false
                });

            await uow.CommitAsync();

            return (result, validFromUtc, validToUtc);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            UserResourceOwnerV1(IIdentityUnitOfWork uow, tbl_Issuers issuer, tbl_Clients client, tbl_Users user)
        {
            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTime.UtcNow;
            var validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.InstanceType == InstanceContext.UnitTest
                && uow.ConfigRepo.ResourceOwnerTokenFake)
            {
                validFromUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerTokenExpire);
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

            await uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV1.ToString(),
                    Immutable = false
                });

            await uow.CommitAsync();

            return (result, validFromUtc, validToUtc);
        }

        public static async Task<(string token, DateTime begin, DateTime end)>
            UserResourceOwnerV2(IIdentityUnitOfWork uow, tbl_Issuers issuer, List<tbl_Clients> clients, tbl_Users user)
        {
            var principal = await uow.UserRepo.GenerateAccessTokenAsync(user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTime.UtcNow;
            var validToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerTokenExpire);

            /*
             * redo with https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.isystemclock
             * because this is gross. prefer removal of test check below and muck with clock in test context. 
             */

            if (uow.InstanceType == InstanceContext.UnitTest
                && uow.ConfigRepo.ResourceOwnerTokenFake)
            {
                validFromUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow;
                validToUtc = uow.ConfigRepo.ResourceOwnerTokenFakeUtcNow.AddSeconds(uow.ConfigRepo.ResourceOwnerTokenExpire);
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

            await uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false
                });

            await uow.CommitAsync();

            return (result, validFromUtc, validToUtc);
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
