using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Bhbk.Lib.Identity.Factories
{
    public enum AudienceType
    {
        user_agent,
        native,
        server,
    }

    public class OAuth2JwtFactory : IOAuth2JwtFactory
    {
        public const string _authType = "JWT";

        public OAuth2JwtFactory() { }

        public JwtSecurityToken ClientCredential(KeyValuePair<string, string> issuer, string issuerSalt,
            string audience, KeyValuePair<string, List<string>> claims)
        {
            return ClientCredential(issuer.Key, issuer.Value, issuerSalt, audience, GenerateClaims(claims));
        }

        public JwtSecurityToken ClientCredential(string issuer, string issuerKey, string issuerSalt,
            string audience, List<Claim> claims)
        {
            var symmetricKeyAsBase64 = issuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;

            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            //service identity vs. a user identity
            claims.Add(new Claim(ClaimTypes.System, AudienceType.server.ToString()));

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, _authType));

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: string.Format("{0}:{1}", issuer, issuerSalt),
                    audience: audience,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
                    ));

            return new JwtSecurityToken(result);
        }

        public JwtSecurityToken ResourceOwnerPassword(KeyValuePair<string, string> issuer, string issuerSalt,
            List<string> audiences, KeyValuePair<string, List<string>> claims)
        {
            return ResourceOwnerPassword(issuer.Key, issuer.Value, issuerSalt, audiences, GenerateClaims(claims));
        }

        public JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string issuerSalt,
            List<string> audiences, List<Claim> claims)
        {
            var symmetricKeyAsBase64 = issuerKey;
            var keyBytes = Encoding.Unicode.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var validFromUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Nbf).Single().Value)).UtcDateTime;

            var validToUtc = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).Single().Value)).UtcDateTime;

            //user identity vs. a service identity
            claims.Add(new Claim(ClaimTypes.System, AudienceType.user_agent.ToString()));

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, _authType));

            string issuerResult = string.Empty;

            if (string.IsNullOrEmpty(issuerSalt))
                issuerResult = issuer;
            else
                issuerResult = string.Format("{0}:{1}", issuer, issuerSalt);

            string audienceResult = string.Empty;

            if (audiences.Count == 0)
                throw new InvalidOperationException();
            else
                audienceResult = string.Join(", ", audiences.OrderBy(x => x));

            SigningCredentials signingCredentialResult = null;

            if (string.IsNullOrEmpty(issuerSalt))
                signingCredentialResult = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            else
                signingCredentialResult = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512);

            var result = new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: issuerResult,
                    audience: audienceResult,
                    claims: principal.Claims,
                    notBefore: validFromUtc,
                    expires: validToUtc,
                    signingCredentials: signingCredentialResult
                    ));

            return new JwtSecurityToken(result);
        }

        public JwtSecurityToken Parse(string jwt)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        }

        public bool Valid(string jwt)
        {
            return new JwtSecurityTokenHandler().CanReadToken(jwt);
        }

        private List<Claim> GenerateClaims(KeyValuePair<string, List<string>> claims)
        {
            var principal = new List<Claim>();

            principal.Add(new Claim(ClaimTypes.NameIdentifier, claims.Key));

            foreach (var role in claims.Value)
                principal.Add(new Claim(ClaimTypes.Role, role));

            //not before timestamp
            principal.Add(new Claim(JwtRegisteredClaimNames.Nbf, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            principal.Add(new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            principal.Add(new Claim(JwtRegisteredClaimNames.Exp, 
                new DateTimeOffset(DateTime.UtcNow).AddSeconds(86400).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return principal;
        }
    }
}
