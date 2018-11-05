using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Interfaces
{
    public interface IJwtContext
    {
        JwtSecurityToken AccessToken { get; }
    }
}
