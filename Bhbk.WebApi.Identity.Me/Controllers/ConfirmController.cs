using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
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
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!await new ProtectProvider(UoW.InstanceType.ToString()).ValidateAsync(email, token, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            await UoW.UserRepo.SetConfirmedEmailAsync(user.Id, true);

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
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!await new ProtectProvider(UoW.InstanceType.ToString()).ValidateAsync(password, token, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            await UoW.UserRepo.SetConfirmedPasswordAsync(user.Id, true);

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
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!await new TotpProvider(8, 10).ValidateAsync(phoneNumber, token, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            await UoW.UserRepo.SetConfirmedPhoneNumberAsync(user.Id, true);

            await UoW.CommitAsync();

            return NoContent();
        }
    }
}
