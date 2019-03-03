using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Internal.Interfaces
{
    public interface IJwtContext
    {
        JwtSecurityToken AccessToken { get; }
    }
}
