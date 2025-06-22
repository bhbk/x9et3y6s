using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Grants
{
    public interface IOAuth2JwtGrant
    {
        JwtSecurityToken AccessToken { get; set; }
        ValueTask<JwtSecurityToken> AccessTokenAsync { get; set; }
    }
}
