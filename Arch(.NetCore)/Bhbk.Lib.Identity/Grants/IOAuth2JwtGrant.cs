using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Grants
{
    public interface IOAuth2JwtGrant
    {
        JwtSecurityToken AccessToken { get; set; }
    }
}
