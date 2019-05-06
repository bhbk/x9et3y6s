using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Helpers
{
    public interface IClientCredentialGrant
    {
        JwtSecurityToken CcgV2 { get; }
    }
}
