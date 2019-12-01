using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Factories
{
    public interface IOAuth2JwtFactory
    {
        JwtSecurityToken ClientCredential(KeyValuePair<string, string> issuer, string issuerSalt,
            string audience, KeyValuePair<string, List<string>> claims);

        JwtSecurityToken ClientCredential(string issuer, string issuerKey, string issuerSalt,
            string audience, List<Claim> claims);

        JwtSecurityToken ResourceOwnerPassword(KeyValuePair<string, string> issuer, string issuerSalt,
            List<string> audiences, KeyValuePair<string, List<string>> claims);

        JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string issuerSalt,
            List<string> audiences, List<Claim> claims);

        JwtSecurityToken Parse(string jwt);

        bool Valid(string jwt);
    }
}
