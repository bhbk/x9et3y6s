using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("detail")]
    public class DetailController : BaseController
    {
        public DetailController() { }

        public DetailController(IIdentityContext context)
            : base(context) { }

        [Route("v1/change-email"), HttpPut]
        public async Task<IActionResult> AskChangeEmail(UserChangeEmail model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Email != model.CurrentEmail)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (await Context.UserMgmt.GetEmailAsync(user) != user.Email)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (model.NewEmail != model.NewEmailConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidEmailConfirm);

            else
            {
                string token = await Context.UserMgmt.GenerateEmailConfirmationTokenAsync(user);

                if (Context.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    //await Context.UserMgmt.SendEmailAsync(user.Id, "Confirmation Email", "Confirmatil Email Body " + token);
                    return NoContent();
                }
            }
        }

        [Route("v1/change-password"), HttpPut]
        public async Task<IActionResult> AskChangePassword(UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await Context.UserMgmt.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                string token = await Context.UserMgmt.GeneratePasswordResetTokenAsync(user);

                if (Context.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    //await Context.UserMgmt.SendEmailAsync(user.Id, "Confirmation Email", "Confirmation Email Body " + token);
                    return NoContent();
                }
            }
        }

        [Route("v1/change-phone"), HttpPut]
        public async Task<IActionResult> AskChangePhone(UserChangePhone model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (await Context.UserMgmt.GetPhoneNumberAsync(user) != model.CurrentPhoneNumber)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPhone);

            else if (model.NewPhoneNumber != model.NewPhoneNumberConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPhoneConfirm);

            else
            {
                string token = await Context.UserMgmt.GenerateChangePhoneNumberTokenAsync(user, model.NewPhoneNumber);

                if (Context.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    //await Context.UserMgmt.SendSmsAsync(user.Id, "Confirmation Code" + token);
                    return NoContent();
                }
            }
        }

        [Route("v1/claims"), HttpGet]
        public IActionResult GetClaims()
        {
            var user = User.Identity as ClaimsIdentity;

            return Ok(user.Claims);
        }

        [Route("v1/two-factor/{status}"), HttpPut]
        public async Task<IActionResult> SetTwoFactor(bool status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            bool current = await Context.UserMgmt.GetTwoFactorEnabledAsync(user);

            if (current == status)
                return BadRequest(BaseLib.Statics.MsgUserTwoFactorAlreadyExists);

            else
            {
                var result = await Context.UserMgmt.SetTwoFactorEnabledAsync(user, status);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1"), HttpPut]
        public async Task<IActionResult> UpdateDetail(UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                var update = new UserFactory<UserUpdate>(model);
                var result = await Context.UserMgmt.UpdateAsync(update.Devolve());

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update.Evolve());
            }
        }
    }
}
