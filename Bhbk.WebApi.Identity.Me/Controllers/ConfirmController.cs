using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("confirm")]
    public class ConfirmController : BaseController
    {
        public ConfirmController() { }

        public ConfirmController(IIdentityContext context)
            : base(context) { }

        [Route("v1/set-email/{userId}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(Guid userId, [FromBody]string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await Context.UserMgmt.VerifyUserTokenAsync(user, string.Empty, BaseLib.Statics.ApiTokenConfirmEmail, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                var result = await Context.UserMgmt.SetEmailConfirmedAsync(user, true);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/set-password/{userId}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPassword(Guid userId, [FromBody]string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await Context.UserMgmt.VerifyUserTokenAsync(user, string.Empty, BaseLib.Statics.ApiTokenResetPassword, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                var result = await Context.UserMgmt.SetPasswordConfirmedAsync(user, true);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/set-phone/{userId}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhone(Guid userId, [FromBody]string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await Context.UserMgmt.VerifyChangePhoneNumberTokenAsync(user, token, user.PhoneNumber))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                var result = await Context.UserMgmt.SetPhoneNumberConfirmedAsync(user, true);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/change-email/{token}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailChange(UserChangeEmail model, string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await Context.UserMgmt.VerifyUserTokenAsync(user, string.Empty, BaseLib.Statics.ApiTokenConfirmEmail, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                var result = await Context.UserMgmt.ConfirmEmailAsync(user, token);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/change-password/{token}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPasswordChange(UserChangePassword model, string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else if (!await Context.UserMgmt.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (!await Context.UserMgmt.VerifyUserTokenAsync(user, string.Empty, BaseLib.Statics.ApiTokenResetPassword, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                var result = await Context.UserMgmt.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/change-phone/{token}"), HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhoneChange(UserChangePhone model, string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await Context.UserMgmt.VerifyChangePhoneNumberTokenAsync(user, token, model.CurrentPhoneNumber))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                var result = await Context.UserMgmt.ChangePhoneNumberAsync(user, model.NewPhoneNumber, token);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }
    }
}