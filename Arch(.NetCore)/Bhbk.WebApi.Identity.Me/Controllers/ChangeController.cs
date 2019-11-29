﻿using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Providers.Me;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("change")]
    public class ChangeController : BaseController
    {
        private ChangeProvider _provider;

        public ChangeController(IConfiguration conf, IContextService instance)
        {
            _provider = new ChangeProvider(conf, instance);
        }

        [Route("v1/email"), HttpPut]
        public IActionResult ChangeEmailV1([FromBody] EntityChangeEmail model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.EntityId}");
                return NotFound(ModelState);
            }
            else if (user.Id != model.EntityId
                || user.Email != model.CurrentEmail
                || model.NewEmail != model.NewEmailConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var expire = UoW.Settings.Get(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalTotpExpire).Single();

            string token = HttpUtility.UrlEncode(new PasswordlessTokenFactory(UoW.InstanceType.ToString())
                .Generate(model.NewEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user));

            if (UoW.InstanceType == InstanceContext.UnitTest)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmEmailV1(Conf, user, token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            alert.Email_EnqueueV1(new EmailCreate()
            {
                FromId = user.Id,
                FromEmail = user.Email,
                FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                ToId = user.Id,
                ToEmail = user.Email,
                ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                Subject = string.Format("{0}", Constants.MsgConfirmEmailSubject),
                HtmlContent = Constants.TemplateConfirmEmail(user, url)
            });

            return NoContent();
        }

        [Route("v1/password"), HttpPut]
        public IActionResult ChangePasswordV1([FromBody] EntityChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.EntityId}");
                return NotFound(ModelState);
            }
            else if (!user.HumanBeing
                || user.Id != model.EntityId)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }
            else if (!UoW.Users.VerifyPassword(user, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            var expire = UoW.Settings.Get(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalTotpExpire).Single();

            string token = HttpUtility.UrlEncode(new PasswordlessTokenFactory(UoW.InstanceType.ToString())
                .Generate(model.NewPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user));

            if (UoW.InstanceType == InstanceContext.UnitTest)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmPasswordV1(Conf, user, token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            alert.Email_EnqueueV1(new EmailCreate()
            {
                FromId = user.Id,
                FromEmail = user.Email,
                FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                ToId = user.Id,
                ToEmail = user.Email,
                ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                Subject = string.Format("{0}", Constants.MsgConfirmPasswordSubject),
                HtmlContent = Constants.TemplateConfirmPassword(user, url)
            });

            return NoContent();
        }

        [Route("v1/phone"), HttpPut]
        public IActionResult ChangePhoneV1([FromBody] EntityChangePhone model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.EntityId}");
                return NotFound(ModelState);
            }
            else if (user.Id != model.EntityId
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

            string token = HttpUtility.UrlEncode(new TimeBasedTokenFactory(8, 10).Generate(model.NewPhoneNumber, user));

            if (UoW.InstanceType == InstanceContext.UnitTest)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmPasswordV1(Conf, user, token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            alert.Text_EnqueueV1(new TextCreate()
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