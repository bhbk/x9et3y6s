using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Repositories;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Services
{
    public interface IMeService
    {
        JwtSecurityToken Jwt { get; set; }
        MeRepository Repo { get; }

        UserModel Detail_GetV1();
    }
}
