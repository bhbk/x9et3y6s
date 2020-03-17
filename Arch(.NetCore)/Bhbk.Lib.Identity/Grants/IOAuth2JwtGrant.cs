using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Grants
{
    public interface IOAuth2JwtGrant
    {
        JwtSecurityToken Jwt { get; set; }
    }
}
