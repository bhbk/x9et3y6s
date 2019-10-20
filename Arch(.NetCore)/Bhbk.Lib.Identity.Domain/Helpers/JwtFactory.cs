using AutoMapper;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Domain.Helpers
{
    public class JwtFactory
    {
        public static async ValueTask<JwtSecurityToken>
            ClientRefreshV2(IUoWService uow, IMapper mapper, tbl_Issuers issuer, tbl_Clients client)
        {
            var principal = await uow.Clients.GenerateRefreshClaimsAsync(issuer, client);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;
            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.Issuers.Salt,
                    audience: null,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            await uow.Refreshes.CreateAsync(
                mapper.Map<tbl_Refreshes>(new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    RefreshType = RefreshType.Client.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc,
                }));

            await uow.Activities.CreateAsync(
                mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    ClientId = client.Id,
                    ActivityType = LoginType.CreateClientRefreshTokenV2.ToString(),
                    Immutable = false
                }));

            return new JwtSecurityToken(result);
        }

        public static async ValueTask<JwtSecurityToken>
            ClientResourceOwnerV2(IUoWService uow, IMapper mapper, tbl_Issuers issuer, tbl_Clients client)
        {
            var principal = await uow.Clients.GenerateAccessClaimsAsync(issuer, client);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;
            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.Issuers.Salt,
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            await uow.Activities.CreateAsync(
                mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    ClientId = client.Id,
                    ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                    Immutable = false
                }));

            return new JwtSecurityToken(result);
        }

        public static async ValueTask<JwtSecurityToken>
            UserRefreshV1(IUoWService uow, IMapper mapper, tbl_Issuers issuer, tbl_Users user)
        {
            var principal = await uow.Users.GenerateRefreshClaimsAsync(issuer, user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;
            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.Issuers.Salt,
                    audience: null,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            await uow.Refreshes.CreateAsync(
                mapper.Map<tbl_Refreshes>(new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc,
                }));

            await uow.Activities.CreateAsync(
                mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV1.ToString(),
                    Immutable = false
                }));

            return new JwtSecurityToken(result);
        }

        public static async ValueTask<JwtSecurityToken>
            UserRefreshV2(IUoWService uow, IMapper mapper, tbl_Issuers issuer, tbl_Users user)
        {
            var principal = await uow.Users.GenerateRefreshClaimsAsync(issuer, user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;
            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.Issuers.Salt,
                    audience: null,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            await uow.Refreshes.CreateAsync(
                mapper.Map<tbl_Refreshes>(new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = result,
                    ValidFromUtc = validFromUtc,
                    ValidToUtc = validToUtc
                }));

            await uow.Activities.CreateAsync(
                mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV2.ToString(),
                    Immutable = false
                }));

            return new JwtSecurityToken(result);
        }

        public static async ValueTask<JwtSecurityToken>
            UserResourceOwnerV1_Legacy(IUoWService uow, IMapper mapper, tbl_Issuers issuer, tbl_Clients client, tbl_Users user)
        {
            var principal = await uow.Users.GenerateAccessClaimsAsync(issuer, user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;
            var validToUtc = validFromUtc.AddSeconds(86400);

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString(),
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            await uow.Activities.CreateAsync(
                mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV1Legacy.ToString(),
                    Immutable = false
                }));

            return new JwtSecurityToken(result);
        }

        public static async ValueTask<JwtSecurityToken>
            UserResourceOwnerV1(IUoWService uow, IMapper mapper, tbl_Issuers issuer, tbl_Clients client, tbl_Users user)
        {
            var principal = await uow.Users.GenerateAccessClaimsAsync(issuer, user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;
            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.Issuers.Salt,
                    audience: client.Name.ToString(),
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            await uow.Activities.CreateAsync(
                mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV1.ToString(),
                    Immutable = false
                }));

            return new JwtSecurityToken(result);
        }

        public static async ValueTask<JwtSecurityToken>
            UserResourceOwnerV2(IUoWService uow, IMapper mapper, tbl_Issuers issuer, List<tbl_Clients> clients, tbl_Users user)
        {
            var principal = await uow.Users.GenerateAccessClaimsAsync(issuer, user);

            var symmetricKeyAsBase64 = issuer.IssuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;
            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            string clientList = string.Empty;

            if (clients.Count == 0)
                throw new InvalidOperationException();

            else
                clientList = string.Join(", ", clients.Select(x => x.Name.ToString()).OrderBy(x => x));

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer.Name.ToString() + ":" + uow.Issuers.Salt,
                    audience: clientList,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            await uow.Activities.CreateAsync(
                mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false
                }));

            return new JwtSecurityToken(result);
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
