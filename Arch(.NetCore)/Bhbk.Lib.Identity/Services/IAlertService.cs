using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public interface IAlertService
    {
        JwtSecurityToken Jwt { get; set; }
        AlertRepository Http { get; }

        ValueTask<bool> Email_EnqueueV1(EmailV1 model);
        ValueTask<bool> Text_EnqueueV1(TextV1 model);
    }
}
