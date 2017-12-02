using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
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
            
            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Email != model.CurrentEmail)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (await Context.UserMgmt.GetEmailAsync(user.Id) != user.Email)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (model.NewEmail != model.NewEmailConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidEmailConfirm);

            else
            {
                string token = await Context.UserMgmt.GenerateEmailConfirmationTokenAsync(user.Id);

                if (Context.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    //await context.UserMgmt.SendEmailAsync(user.Id, "Confirmation Email", "Confirmatil Email Body " + token);
                    return Ok();
                }
            }
        }

        [Route("v1/change-password"), HttpPut]
        public async Task<IActionResult> AskChangePassword(UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await Context.UserMgmt.CheckPasswordAsync(user.Id, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                string token = await Context.UserMgmt.GeneratePasswordResetTokenAsync(user.Id);

                if (Context.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    //await Bubble.UserMgmt.SendEmailAsync(user.Id, "Confirmation Email", "Confirmation Email Body " + token);
                    return Ok();
                }
            }
        }

        [Route("v1/change-phone"), HttpPut]
        public async Task<IActionResult> AskChangePhone(UserChangePhone model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (await Context.UserMgmt.GetPhoneNumberAsync(user.Id) != model.CurrentPhoneNumber)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPhone);

            else if (model.NewPhoneNumber != model.NewPhoneNumberConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPhoneConfirm);

            else
            {
                string token = await Context.UserMgmt.GenerateChangePhoneNumberTokenAsync(user.Id, model.NewPhoneNumber);

                if (Context.ContextStatus == ContextType.UnitTest)
                    return Ok(token);
                else
                {
                    //await Bubble.UserMgmt.SendSmsAsync(user.Id, "Confirmation Code" + token);
                    return Ok();
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

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            bool current = await Context.UserMgmt.GetTwoFactorEnabledAsync(user.Id);

            if (current == status)
                return BadRequest(BaseLib.Statics.MsgUserTwoFactorAlreadyExists);

            else
            {
                IdentityResult result = await Context.UserMgmt.SetTwoFactorEnabledAsync(user.Id, status);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpPut]
        public async Task<IActionResult> UpdateDetail(UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(GetUserGUID());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                model.LockoutEnabled = user.LockoutEnabled;
                model.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;

                var update = Context.UserMgmt.Store.Mf.Update.DoIt(model);
                var devolve = Context.UserMgmt.Store.Mf.Devolve.DoIt(update);
                IdentityResult result = await Context.UserMgmt.UpdateAsync(devolve);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update);
            }
        }
    }
}
