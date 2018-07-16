using Bhbk.Lib.Alert.Factory;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Web;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("change")]
    public class ChangeController : BaseController
    {
        public ChangeController() { }

        public ChangeController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/email"), HttpPut]
        public async Task<IActionResult> ChangeEmailV1([FromBody] UserChangeEmail model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Email != model.CurrentEmail)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (model.NewEmail != model.NewEmailConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidEmailConfirm);

            string token = HttpUtility.UrlEncode(await new ProtectProvider(IoC.ContextStatus.ToString())
                .GenerateAsync(model.NewEmail, TimeSpan.FromSeconds(IoC.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

            if (IoC.ContextStatus == ContextType.UnitTest)
                return Ok(token);

            var url = UrlBuilder.UiConfirmEmail(Conf, user, token);

            var spam = await alert.SendEmailV1(Jwt,
                new EmailCreate()
                {
                    FromId = user.Id,
                    FromEmail = user.Email,
                    FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    ToId = user.Id,
                    ToEmail = user.Email,
                    ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    Subject = string.Format("{0}", BaseLib.Statics.ApiEmailConfirmEmailSubject),
                    HtmlContent = BaseLib.Statics.ApiEmailConfirmEmailHtml(user, url)
                });

            if (!spam.IsSuccessStatusCode)
                return BadRequest(BaseLib.Statics.MsgSysQueueEmailError);

            return NoContent();
        }

        [Route("v1/password"), HttpPut]
        public async Task<IActionResult> ChangePasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await IoC.UserMgmt.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            string token = HttpUtility.UrlEncode(await new ProtectProvider(IoC.ContextStatus.ToString())
                .GenerateAsync(model.NewPassword, TimeSpan.FromSeconds(IoC.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

            if (IoC.ContextStatus == ContextType.UnitTest)
                return Ok(token);

            var url = UrlBuilder.UiConfirmPassword(Conf, user, token);

            var spam = await alert.SendEmailV1(Jwt,
                new EmailCreate()
                {
                    FromId = user.Id,
                    FromEmail = user.Email,
                    FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    ToId = user.Id,
                    ToEmail = user.Email,
                    ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    Subject = string.Format("{0}", BaseLib.Statics.ApiEmailConfirmPasswordSubject),
                    HtmlContent = BaseLib.Statics.ApiEmailConfirmPasswordHtml(user, url)
                });

            if (!spam.IsSuccessStatusCode)
                return BadRequest(BaseLib.Statics.MsgSysQueueEmailError);

            return NoContent();
        }

        [Route("v1/phone"), HttpPut]
        public async Task<IActionResult> ChangePhoneV1([FromBody] UserChangePhone model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.PhoneNumber != model.CurrentPhoneNumber)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPhone);

            else if (model.NewPhoneNumber != model.NewPhoneNumberConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPhoneConfirm);

            string token = HttpUtility.UrlEncode(await new TotpProvider(8, 10).GenerateAsync(model.NewPhoneNumber, user));

            if (IoC.ContextStatus == ContextType.UnitTest)
                return Ok(token);

            var url = UrlBuilder.UiConfirmPassword(Conf, user, token);

            var spam = await alert.SendTextV1(Jwt,
                new SmsCreate()
                {
                    FromId = user.Id,
                    FromPhoneNumber = model.NewPhoneNumber,
                    ToId = user.Id,
                    ToPhoneNumber = model.NewPhoneNumber,
                    Body = token
                });

            if (!spam.IsSuccessStatusCode)
                return BadRequest(BaseLib.Statics.MsgSysQueueEmailError);

            return NoContent();
        }
    }
}