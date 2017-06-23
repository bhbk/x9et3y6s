using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controller
{
    [RoutePrefix("detail")]
    public class DetailController : BaseController
    {
        public DetailController() { }

        public DetailController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/change-email"), HttpPut]
        public async Task<IHttpActionResult> AskChangeEmail(UserModel.ChangeEmail model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Email != model.CurrentEmail)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (await UoW.UserMgmt.GetEmailAsync(user.Id) != user.Email)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (model.NewEmail != model.NewEmailConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidEmailConfirm);

            else
            {
                string token = await UoW.UserMgmt.GenerateEmailConfirmationTokenAsync(user.Id);

                if (UoW.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    await UoW.UserMgmt.SendEmailAsync(user.Id, "Confirmation Email", "Confirmatil Email Body " + token);
                    return Ok();
                }
            }
        }

        [Route("v1/change-password"), HttpPut]
        public async Task<IHttpActionResult> AskChangePassword(UserModel.ChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsyncDeprecated(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await UoW.UserMgmt.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                string token = await UoW.UserMgmt.GeneratePasswordResetTokenAsync(user.Id);

                if (UoW.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    await UoW.UserMgmt.SendEmailAsync(user.Id, "Confirmation Email", "Confirmation Email Body " + token);
                    return Ok();
                }
            }
        }

        [Route("v1/change-phone"), HttpPut]
        public async Task<IHttpActionResult> AskChangePhone(UserModel.ChangePhone model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (await UoW.UserMgmt.GetPhoneNumberAsync(user.Id) != model.CurrentPhoneNumber)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPhone);

            else if (model.NewPhoneNumber != model.NewPhoneNumberConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPhoneConfirm);

            else
            {
                string token = await UoW.UserMgmt.GenerateChangePhoneNumberTokenAsync(user.Id, model.NewPhoneNumber);

                if (UoW.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    await UoW.UserMgmt.SendSmsAsync(user.Id, "Confirmation Code" + token);
                    return Ok();
                }
            }
        }

        [Route("v1/two-factor/{status}"), HttpPut]
        public async Task<IHttpActionResult> SetTwoFactor(bool status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            bool current = await UoW.UserMgmt.GetTwoFactorEnabledAsync(user.Id);

            if (current == status)
                return BadRequest(BaseLib.Statics.MsgUserTwoFactorAlreadyExists);

            else
            {
                var result = await UoW.UserMgmt.SetTwoFactorEnabledAsync(user.Id, status);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpPut]
        public async Task<IHttpActionResult> UpdateDetail(UserModel.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                IdentityResult result = await UoW.UserMgmt.UpdateAsync(UoW.Models.Devolve.DoIt(user));

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(user);
            }
        }
    }
}
