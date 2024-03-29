﻿using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Templates;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("change")]
    [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
    public class ChangeController : BaseController
    {
        [Route("v1/email"), HttpPut]
        public async ValueTask<IActionResult> ChangeEmailV1([FromBody] EmailChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.EntityId}");
                return NotFound(ModelState);
            }
            else if (user.Id != model.EntityId
                || user.EmailAddress != model.CurrentEmail
                || model.NewEmail != model.NewEmailConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalTotpExpire).Single();

            string token = HttpUtility.UrlEncode(new PasswordTokenFactory(uow.InstanceType.ToString())
                .Generate(model.NewEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user.Id.ToString(), user.SecurityStamp));

            if (uow.InstanceType != InstanceContext.DeployedOrLocal
                && uow.InstanceType != InstanceContext.End2EndTest)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmEmailV1(conf, user.Id.ToString(), token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            await alert.Enqueue_EmailV1(
                new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    FromDisplay = $"{user.FirstName} {user.LastName}",
                    ToEmail = user.EmailAddress,
                    ToDisplay = $"{user.FirstName} {user.LastName}",
                    Subject = MessageConstants.ConfirmEmailSubject,
                    Body = Email.ConfirmEmail(map.Map<UserV1>(user), url)
                });

            return NoContent();
        }

        [Route("v1/password"), HttpPut]
        public async ValueTask<IActionResult> ChangePasswordV1([FromBody] PasswordChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.EntityId}");
                return NotFound(ModelState);
            }
            else if (!user.IsHumanBeing
                || user.Id != model.EntityId)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }
            else if (!PBKDF2.Validate(user.PasswordHashPBKDF2, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalTotpExpire).Single();

            string token = HttpUtility.UrlEncode(new PasswordTokenFactory(uow.InstanceType.ToString())
                .Generate(model.NewPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user.Id.ToString(), user.SecurityStamp));

            if (uow.InstanceType != InstanceContext.DeployedOrLocal
                && uow.InstanceType != InstanceContext.End2EndTest)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmPasswordV1(conf, user.Id.ToString(), token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            await alert.Enqueue_EmailV1(
                new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    FromDisplay = $"{user.FirstName} {user.LastName}",
                    ToEmail = user.EmailAddress,
                    ToDisplay = $"{user.FirstName} {user.LastName}",
                    Subject = MessageConstants.ConfirmPasswordSubject,
                    Body = Email.ConfirmPassword(map.Map<UserV1>(user), url)
                });

            return NoContent();
        }

        [Route("v1/phone"), HttpPut]
        public async ValueTask<IActionResult> ChangePhoneV1([FromBody] PhoneChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.EntityId}");
                return NotFound(ModelState);
            }
            else if (user.Id != model.EntityId
                || !user.IsHumanBeing)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }
            else if (user.PhoneNumber != model.CurrentPhoneNumber
                || model.NewPhoneNumber != model.NewPhoneNumberConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad phone for user:{user.Id}");
                return BadRequest(ModelState);
            }

            string token = HttpUtility.UrlEncode(new TimeBasedTokenFactory(8, 10).Generate(model.NewPhoneNumber, user.Id.ToString()));

            if (uow.InstanceType != InstanceContext.DeployedOrLocal
                && uow.InstanceType != InstanceContext.End2EndTest)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmPasswordV1(conf, user.Id.ToString(), token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            await alert.Enqueue_TextV1(
                new TextV1()
                {
                    FromPhoneNumber = model.NewPhoneNumber,
                    ToPhoneNumber = model.NewPhoneNumber,
                    Body = token
                });

            return NoContent();
        }
    }
}