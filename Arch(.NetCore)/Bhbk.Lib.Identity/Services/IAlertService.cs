using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public interface IAlertService
    {
        JwtSecurityToken Jwt { get; set; }
        AlertRepository Endpoints { get; }
        IOAuth2JwtGrant Grant { get; set; }

        ValueTask<bool> Dequeue_EmailV1(Guid emailID);
        ValueTask<bool> Dequeue_TextV1(Guid textID);
        ValueTask<bool> Enqueue_EmailV1(EmailV1 model);
        ValueTask<bool> Enqueue_TextV1(TextV1 model);
    }
}
