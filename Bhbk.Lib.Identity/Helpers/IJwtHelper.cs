using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Helpers
{
    public interface IJwtHelper
    {
        JwtSecurityToken ResourceOwnerV1 { get; }
        JwtSecurityToken ResourceOwnerV2 { get; }
        JwtSecurityToken ClientCredentialV1 { get; }
        JwtSecurityToken ClientCredentialV2 { get; }
    }
}
