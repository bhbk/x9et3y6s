using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Data.EFCore.Primitives;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Domain.Providers.Me;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy = Lib.Identity.Primitives.Constants.DefaultPolicyForHumans)]
    public class ChangeController : BaseController
    {
        private ChangeProvider _provider;

        public ChangeController(IConfiguration conf, IContextService instance)
        {
            _provider = new ChangeProvider(conf, instance);
        }

        [Route("v1/email"), HttpPut]
        public IActionResult ChangeEmailV1([FromBody] EmailChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

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

            var expire = UoW.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalTotpExpire).Single();

            string token = HttpUtility.UrlEncode(new PasswordTokenFactory(UoW.InstanceType.ToString())
                .Generate(model.NewEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user));

            if (UoW.InstanceType != InstanceContext.DeployedOrLocal)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmEmailV1(Conf, user, token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            alert.Email_EnqueueV1(new EmailV1()
            {
                FromId = user.Id,
                FromEmail = user.EmailAddress,
                FromDisplay = $"{user.FirstName} {user.LastName}",
                ToId = user.Id,
                ToEmail = user.EmailAddress,
                ToDisplay = $"{user.FirstName} {user.LastName}",
                Subject = Constants.MsgConfirmEmailSubject,
                HtmlContent = Templates.ConfirmEmail(user, url)
            });

            return NoContent();
        }

        [Route("v1/password"), HttpPut]
        public IActionResult ChangePasswordV1([FromBody] PasswordChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

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
            else if (!PBKDF2.Validate(user.PasswordHashPBKDF2, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            var expire = UoW.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalTotpExpire).Single();

            string token = HttpUtility.UrlEncode(new PasswordTokenFactory(UoW.InstanceType.ToString())
                .Generate(model.NewPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user));

            if (UoW.InstanceType != InstanceContext.DeployedOrLocal)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmPasswordV1(Conf, user, token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            alert.Email_EnqueueV1(new EmailV1()
            {
                FromId = user.Id,
                FromEmail = user.EmailAddress,
                FromDisplay = $"{user.FirstName} {user.LastName}",
                ToId = user.Id,
                ToEmail = user.EmailAddress,
                ToDisplay = $"{user.FirstName} {user.LastName}",
                Subject = Constants.MsgConfirmPasswordSubject,
                HtmlContent = Templates.ConfirmPassword(user, url)
            });

            return NoContent();
        }

        [Route("v1/phone"), HttpPut]
        public IActionResult ChangePhoneV1([FromBody] PhoneChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

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

            if (UoW.InstanceType != InstanceContext.DeployedOrLocal)
                return Ok(token);

            var url = UrlFactory.GenerateConfirmPasswordV1(Conf, user, token);
            var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

            alert.Text_EnqueueV1(new TextV1()
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