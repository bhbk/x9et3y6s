using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("confirm")]
    [AllowAnonymous]
    public class ConfirmController : BaseController
    {
        public ConfirmController() { }

        public ConfirmController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }


        [Route("v1/email/{userID:guid}"), HttpPut]
        public async Task<IActionResult> ConfirmEmailV1([FromRoute] Guid userID, [FromBody] string email, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (!await new ProtectProvider(UoW.Situation.ToString()).ValidateAsync(email, token, user))
                return BadRequest(Strings.MsgUserInvalidToken);

            await UoW.CustomUserMgr.Store.SetEmailConfirmedAsync(user, true);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/password/{userID:guid}"), HttpPut]
        public async Task<IActionResult> ConfirmPasswordV1([FromRoute] Guid userID, [FromBody] string password, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (!await new ProtectProvider(UoW.Situation.ToString()).ValidateAsync(password, token, user))
                return BadRequest(Strings.MsgUserInvalidToken);

            await UoW.CustomUserMgr.Store.SetPasswordConfirmedAsync(user, true);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/phone/{userID:guid}"), HttpPut]
        public async Task<IActionResult> ConfirmPhoneV1([FromRoute] Guid userID, [FromBody] string phoneNumber, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (!await new TotpProvider(8, 10).ValidateAsync(phoneNumber, token, user))
                return BadRequest(Strings.MsgUserInvalidToken);

            await UoW.CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);

            await UoW.CommitAsync();

            return NoContent();
        }
    }
}
