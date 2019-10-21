using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Factories
{
    public interface IJsonWebTokenFactory
    {
        JwtSecurityToken ClientCredential(string issuer, string issuerKey, string issuerSalt, string client, List<Claim> claims);

        [Obsolete]
        JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string client, List<Claim> claims);

        JwtSecurityToken ResourceOwnerPassword(string issuer, string issuerKey, string issuerSalt, List<string> clients, List<Claim> claims);

        bool CanReadToken(string jwt);

        JwtSecurityToken ReadJwtToken(string jwt);
    }
}
