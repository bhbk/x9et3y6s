using Bhbk.Lib.Alert.Factory;
using Bhbk.Lib.Alert.Helpers;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
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

            var user = await UoW.CustomUserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(Strings.MsgUserInvalid);

            else if (user.Email != model.CurrentEmail)
                return BadRequest(Strings.MsgUserInvalidCurrentEmail);

            else if (model.NewEmail != model.NewEmailConfirm)
                return BadRequest(Strings.MsgUserInvalidEmailConfirm);

            string token = HttpUtility.UrlEncode(await new ProtectProvider(UoW.Situation.ToString())
                .GenerateAsync(model.NewEmail, TimeSpan.FromSeconds(UoW.ConfigRepo.DefaultsAuthorizationCodeExpire), user));

            if (UoW.Situation == ContextType.UnitTest)
                return Ok(token);

            var url = LinkBuilder.ConfirmEmail(Conf, user, token);

            var alert = new AlertClient(Conf, UoW.Situation);

            if (alert == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var email = await alert.EnqueueEmailV1(Jwt.AccessToken,
                new EmailCreate()
                {
                    FromId = user.Id,
                    FromEmail = user.Email,
                    FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    ToId = user.Id,
                    ToEmail = user.Email,
                    ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    Subject = string.Format("{0}", Strings.ApiMsgConfirmEmailSubject),
                    HtmlContent = Strings.ApiTemplateConfirmEmail(user, url)
                });

            if (!email.IsSuccessStatusCode)
                return BadRequest(Strings.MsgSysQueueEmailError);

            return NoContent();
        }

        [Route("v1/password"), HttpPut]
        public async Task<IActionResult> ChangePasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(Strings.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            else if (!await UoW.CustomUserMgr.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(Strings.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            string token = HttpUtility.UrlEncode(await new ProtectProvider(UoW.Situation.ToString())
                .GenerateAsync(model.NewPassword, TimeSpan.FromSeconds(UoW.ConfigRepo.DefaultsAuthorizationCodeExpire), user));

            if (UoW.Situation == ContextType.UnitTest)
                return Ok(token);

            var url = LinkBuilder.ConfirmPassword(Conf, user, token);

            var alert = new AlertClient(Conf, UoW.Situation);

            if (alert == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var email = await alert.EnqueueEmailV1(Jwt.AccessToken,
                new EmailCreate()
                {
                    FromId = user.Id,
                    FromEmail = user.Email,
                    FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    ToId = user.Id,
                    ToEmail = user.Email,
                    ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                    Subject = string.Format("{0}", Strings.ApiMsgConfirmPasswordSubject),
                    HtmlContent = Strings.ApiTemplateConfirmPassword(user, url)
                });

            if (!email.IsSuccessStatusCode)
                return BadRequest(Strings.MsgSysQueueEmailError);

            return NoContent();
        }

        [Route("v1/phone"), HttpPut]
        public async Task<IActionResult> ChangePhoneV1([FromBody] UserChangePhone model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(Strings.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            else if (user.PhoneNumber != model.CurrentPhoneNumber)
                return BadRequest(Strings.MsgUserInvalidCurrentPhone);

            else if (model.NewPhoneNumber != model.NewPhoneNumberConfirm)
                return BadRequest(Strings.MsgUserInvalidPhoneConfirm);

            string token = HttpUtility.UrlEncode(await new TotpProvider(8, 10).GenerateAsync(model.NewPhoneNumber, user));

            if (UoW.Situation == ContextType.UnitTest)
                return Ok(token);

            var url = LinkBuilder.ConfirmPassword(Conf, user, token);

            var alert = new AlertClient(Conf, UoW.Situation);

            if (alert == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var email = await alert.EnqueueTextV1(Jwt.AccessToken,
                new TextCreate()
                {
                    FromId = user.Id,
                    FromPhoneNumber = model.NewPhoneNumber,
                    ToId = user.Id,
                    ToPhoneNumber = model.NewPhoneNumber,
                    Body = token
                });

            if (!email.IsSuccessStatusCode)
                return BadRequest(Strings.MsgSysQueueEmailError);

            return NoContent();
        }
    }
}