using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Providers.Me;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("confirm")]
    public class ConfirmController : BaseController
    {
        private ConfirmProvider _provider;

        public ConfirmController(IConfiguration conf, IContextService instance)
        {
            _provider = new ConfirmProvider(conf, instance);
        }

        [Route("v1/email/{userID:guid}"), HttpPut]
        [AllowAnonymous]
        public async ValueTask<IActionResult> ConfirmEmailV1([FromRoute] Guid userID, [FromBody] string email, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.Users.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!await new ProtectHelper(UoW.InstanceType.ToString()).ValidateAsync(email, token, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            await UoW.Users.SetConfirmedEmailAsync(user, true);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/password/{userID:guid}"), HttpPut]
        [AllowAnonymous]
        public async ValueTask<IActionResult> ConfirmPasswordV1([FromRoute] Guid userID, [FromBody] string password, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.Users.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!await new ProtectHelper(UoW.InstanceType.ToString()).ValidateAsync(password, token, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            await UoW.Users.SetConfirmedPasswordAsync(user, true);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/phone/{userID:guid}"), HttpPut]
        [AllowAnonymous]
        public async ValueTask<IActionResult> ConfirmPhoneV1([FromRoute] Guid userID, [FromBody] string phoneNumber, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.Users.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!await new TotpHelper(8, 10).ValidateAsync(phoneNumber, token, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            await UoW.Users.SetConfirmedPhoneNumberAsync(user, true);
            await UoW.CommitAsync();

            return NoContent();
        }
    }
}
