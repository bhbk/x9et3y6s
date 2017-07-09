using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controller
{
    [RoutePrefix("confirm")]
    public class ConfirmController : BaseController
    {
        public ConfirmController() { }

        public ConfirmController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/set-email/{userId}"), HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ConfirmEmail(Guid userId, [FromBody]string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(userId);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await UoW.UserMgmt.VerifyUserTokenAsync(user.Id, BaseLib.Statics.ApiTokenConfirmEmail, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await UoW.UserMgmt.SetEmailConfirmedAsync(user.Id, true);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/set-password/{userId}"), HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ConfirmPassword(Guid userId, [FromBody]string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(userId);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await UoW.UserMgmt.VerifyUserTokenAsync(user.Id, BaseLib.Statics.ApiTokenResetPassword, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await UoW.UserMgmt.SetPasswordConfirmedAsync(user.Id, true);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/set-phone/{userId}"), HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ConfirmPhone(Guid userId, [FromBody]string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(userId);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await UoW.UserMgmt.VerifyChangePhoneNumberTokenAsync(user.Id, token, user.PhoneNumber))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await UoW.UserMgmt.SetPhoneNumberConfirmedAsync(user.Id, true);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/change-email/{token}"), HttpPut]
        public async Task<IHttpActionResult> ConfirmEmailChange(UserChangeEmail model, string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await UoW.UserMgmt.VerifyUserTokenAsync(user.Id, BaseLib.Statics.ApiTokenConfirmEmail, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await UoW.UserMgmt.ConfirmEmailAsync(user.Id, token);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/change-password/{token}"), HttpPut]
        public async Task<IHttpActionResult> ConfirmPasswordChange(UserChangePassword model, string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else if (!await UoW.UserMgmt.CheckPasswordAsync(user.Id, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (!await UoW.UserMgmt.VerifyUserTokenAsync(user.Id, BaseLib.Statics.ApiTokenResetPassword, token))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await UoW.UserMgmt.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/change-phone/{token}"), HttpPut]
        public async Task<IHttpActionResult> ConfirmPhoneChange(UserChangePhone model, string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await UoW.UserMgmt.VerifyChangePhoneNumberTokenAsync(user.Id, token, model.CurrentPhoneNumber))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await UoW.UserMgmt.ChangePhoneNumberAsync(user.Id, model.NewPhoneNumber, token);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }
    }
}