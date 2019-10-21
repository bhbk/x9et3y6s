using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Bhbk.Lib.Identity.Factories
{
    public class JsonWebTokenFactory : IJsonWebTokenFactory
    {
        private readonly InstanceContext _instance;
        private readonly string _type;

        public JsonWebTokenFactory()
            : this(new ContextService(InstanceContext.DeployedOrLocal)) { }

        public JsonWebTokenFactory(IContextService instance)
        {
            _instance = instance.InstanceType;

            if (_instance == InstanceContext.DeployedOrLocal)
                _type = "JWT:" + InstanceContext.DeployedOrLocal.ToString();

            else if (_instance == InstanceContext.UnitTest)
                _type = "JWT:" + InstanceContext.UnitTest.ToString();

            else
                throw new NotImplementedException();
        }

        public JwtSecurityToken ClientCredential(string issuer, string issuerKey, string issuerSalt, string client, List<Claim> claims)
        {
            var symmetricKeyAsBase64 = issuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;

            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var identity = new ClaimsIdentity(claims, _type);
            var principal = new ClaimsPrincipal(identity);

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer + ":" + issuerSalt,
                    audience: client,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return new JwtSecurityToken(result);
        }

        [Obsolete]
        public JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string client, List<Claim> claims)
        {
            var symmetricKeyAsBase64 = issuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;

            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var identity = new ClaimsIdentity(claims, _type);
            var principal = new ClaimsPrincipal(identity);

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer,
                    audience: client,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ));

            return new JwtSecurityToken(result);
        }

        public JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string issuerSalt, List<string> clients, List<Claim> claims)
        {
            var symmetricKeyAsBase64 = issuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;

            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            var identity = new ClaimsIdentity(claims, _type);
            var principal = new ClaimsPrincipal(identity);

            string clientList = string.Empty;

            if (clients.Count == 0)
                throw new InvalidOperationException();

            else
                clientList = string.Join(", ", clients.OrderBy(x => x));

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuer + ":" + issuerSalt,
                    audience: clientList,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            return new JwtSecurityToken(result);
        }

        public bool CanReadToken(string jwt)
        {
            return new JwtSecurityTokenHandler().CanReadToken(jwt);
        }

        public JwtSecurityToken ReadJwtToken(string jwt)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        }
    }
}
