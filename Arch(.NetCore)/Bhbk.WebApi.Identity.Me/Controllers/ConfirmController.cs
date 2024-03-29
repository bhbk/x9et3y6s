﻿using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Primitives.Constants;
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
    [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
    public class ConfirmController : BaseController
    {
        [Route("v1/email/{userID:guid}"), HttpPut]
        public IActionResult ConfirmEmailV1([FromRoute] Guid userID, [FromBody] string email, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!new PasswordTokenFactory(uow.InstanceType.ToString()).Validate(email, token, user.Id.ToString(), user.SecurityStamp))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            uow.Users.SetConfirmedEmail(user, true);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/password/{userID:guid}"), HttpPut]
        public IActionResult ConfirmPasswordV1([FromRoute] Guid userID, [FromBody] string password, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            else if (!new PasswordTokenFactory(uow.InstanceType.ToString()).Validate(password, token, user.Id.ToString(), user.SecurityStamp))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{token}");
                return BadRequest(ModelState);
            }

            uow.Users.SetConfirmedPassword(user, true);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/phone/{userID:guid}"), HttpPut]
        public IActionResult ConfirmPhoneV1([FromRoute] Guid userID, [FromBody] string phoneNumber, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();

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

            uow.Users.SetConfirmedPhoneNumber(user, true);
            uow.Commit();

            return NoContent();
        }
    }
}
