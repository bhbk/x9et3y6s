using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Helpers
{
    public interface IClientCredentialHelper
    {
        JwtSecurityToken JwtV1 { get; }
        JwtSecurityToken JwtV2 { get; }
    }
}
