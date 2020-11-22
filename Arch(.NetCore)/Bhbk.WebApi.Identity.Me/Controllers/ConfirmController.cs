using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("confirm")]
    [Authorize(Policy = Constants.DefaultPolicyForHumans)]
    public class ConfirmController : BaseController
    {
        [Route("v1/email/{userID:guid}"), HttpPut]
        public IActionResult ConfirmEmailV1([FromRoute] Guid userID, [FromBody] string email, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!new PasswordTokenFactory(UoW.InstanceType.ToString()).Validate(email, token, user.Id.ToString(), user.SecurityStamp))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetConfirmedEmail(user, true);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/password/{userID:guid}"), HttpPut]
        public IActionResult ConfirmPasswordV1([FromRoute] Guid userID, [FromBody] string password, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!new PasswordTokenFactory(UoW.InstanceType.ToString()).Validate(password, token, user.Id.ToString(), user.SecurityStamp))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetConfirmedPassword(user, true);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/phone/{userID:guid}"), HttpPut]
        public IActionResult ConfirmPhoneV1([FromRoute] Guid userID, [FromBody] string phoneNumber, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!new TimeBasedTokenFactory(8, 10).Validate(phoneNumber, token, user.Id.ToString()))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetConfirmedPhoneNumber(user, true);
            UoW.Commit();

            return NoContent();
        }
    }
}
