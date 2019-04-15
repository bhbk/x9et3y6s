using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public interface IJwtContext
    {
        JwtSecurityToken AccessToken { get; }
    }
}
