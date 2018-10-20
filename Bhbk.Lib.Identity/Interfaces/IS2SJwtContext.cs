using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Interfaces
{
    public interface IS2SJwtContext
    {
        JwtSecurityToken AccessToken { get; }
    }
}
