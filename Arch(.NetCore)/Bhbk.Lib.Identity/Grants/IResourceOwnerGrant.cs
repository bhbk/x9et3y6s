using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Grants
{
    public interface IResourceOwnerGrant
    {
        JwtSecurityToken RopgV1 { get; }
        JwtSecurityToken RopgV2 { get; }
    }
}
