using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("confirm")]
    public class ConfirmController : BaseController
    {
        public ConfirmController() { }

        [Route("v1/email/{userID:guid}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailV1([FromRoute] Guid userID, [FromBody] string email, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (!await new ProtectProvider(UoW.Situation.ToString()).ValidateAsync(email, token, user))
                return BadRequest(Strings.MsgUserTokenInvalid);

            await UoW.UserRepo.SetConfirmedEmailAsync(user, true);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/password/{userID:guid}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPasswordV1([FromRoute] Guid userID, [FromBody] string password, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (!await new ProtectProvider(UoW.Situation.ToString()).ValidateAsync(password, token, user))
                return BadRequest(Strings.MsgUserTokenInvalid);

            await UoW.UserRepo.SetConfirmedPasswordAsync(user, true);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/phone/{userID:guid}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhoneV1([FromRoute] Guid userID, [FromBody] string phoneNumber, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (!await new TotpProvider(8, 10).ValidateAsync(phoneNumber, token, user))
                return BadRequest(Strings.MsgUserTokenInvalid);

            await UoW.UserRepo.SetConfirmedPhoneNumberAsync(user, true);

            await UoW.CommitAsync();

            return NoContent();
        }
    }
}
