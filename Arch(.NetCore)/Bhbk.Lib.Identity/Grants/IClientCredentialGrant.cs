using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Grants
{
    public interface IClientCredentialGrant
    {
        JwtSecurityToken CcgV1 { get; }
        JwtSecurityToken CcgV2 { get; }
    }
}
