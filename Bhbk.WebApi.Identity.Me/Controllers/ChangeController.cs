﻿using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("change")]
    public class ChangeController : BaseController
    {
        public ChangeController() { }

        [Route("v1/email"), HttpPut]
        public async Task<IActionResult> ChangeEmailV1([FromBody] UserChangeEmail model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.UserId}");
                return NotFound(ModelState);
            }
            else if (user.Id != model.UserId
                || user.Email != model.CurrentEmail
                || model.NewEmail != model.NewEmailConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            string token = HttpUtility.UrlEncode(await new ProtectProvider(UoW.InstanceType.ToString())
                .GenerateAsync(model.NewEmail, TimeSpan.FromSeconds(UoW.ConfigRepo.AuthCodeTotpExpire), user));

            if (UoW.InstanceType == InstanceContext.UnitTest)
                return Ok(token);

            var url = UrlBuilder.GenerateConfirmEmail(Conf, user, token);

            Alerts.EmailEnqueueV1(new EmailCreate()
            {
                FromId = user.Id,
                FromEmail = user.Email,
                FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                ToId = user.Id,
                ToEmail = user.Email,
                ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                Subject = string.Format("{0}", Strings.MsgConfirmEmailSubject),
                HtmlContent = Strings.TemplateConfirmEmail(user, url)
            });

            return NoContent();
        }

        [Route("v1/password"), HttpPut]
        public async Task<IActionResult> ChangePasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.UserId}");
                return NotFound(ModelState);
            }
            else if (!user.HumanBeing
                || user.Id != model.UserId)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }
            else if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            string token = HttpUtility.UrlEncode(await new ProtectProvider(UoW.InstanceType.ToString())
                .GenerateAsync(model.NewPassword, TimeSpan.FromSeconds(UoW.ConfigRepo.AuthCodeTotpExpire), user));

            if (UoW.InstanceType == InstanceContext.UnitTest)
                return Ok(token);

            var url = UrlBuilder.GenerateConfirmPassword(Conf, user, token);

            Alerts.EmailEnqueueV1(new EmailCreate()
            {
                FromId = user.Id,
                FromEmail = user.Email,
                FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                ToId = user.Id,
                ToEmail = user.Email,
                ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                Subject = string.Format("{0}", Strings.MsgConfirmPasswordSubject),
                HtmlContent = Strings.TemplateConfirmPassword(user, url)
            });

            return NoContent();
        }

        [Route("v1/phone"), HttpPut]
        public async Task<IActionResult> ChangePhoneV1([FromBody] UserChangePhone model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.UserId}");
                return NotFound(ModelState);
            }
            else if (user.Id != model.UserId
                || !user.HumanBeing)
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

            string token = HttpUtility.UrlEncode(await new TotpProvider(8, 10).GenerateAsync(model.NewPhoneNumber, user));

            if (UoW.InstanceType == InstanceContext.UnitTest)
                return Ok(token);

            var url = UrlBuilder.GenerateConfirmPassword(Conf, user, token);

            Alerts.TextEnqueueV1(new TextCreate()
            {
                FromId = user.Id,
                FromPhoneNumber = model.NewPhoneNumber,
                ToId = user.Id,
                ToPhoneNumber = model.NewPhoneNumber,
                Body = token
            });

            return NoContent();
        }
    }
}