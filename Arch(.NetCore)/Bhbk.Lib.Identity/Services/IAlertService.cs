using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Services
{
    public interface IAlertService
    {
        JwtSecurityToken Jwt { get; set; }
        AlertRepository Http { get; }

        bool Email_EnqueueV1(EmailCreate model);
        bool Text_EnqueueV1(TextCreate model);
    }
}
