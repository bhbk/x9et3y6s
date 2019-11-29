using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Factories
{
    public interface IJsonWebTokenFactory
    {
        JwtSecurityToken ClientCredential(string issuer, string issuerKey, string issuerSalt, string audience, List<Claim> claims);

        [Obsolete]
        JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string audience, List<Claim> claims);

        JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string issuerSalt, List<string> audiences, List<Claim> claims);

        bool CanReadToken(string jwt);

        JwtSecurityToken ReadJwtToken(string jwt);
    }
}
