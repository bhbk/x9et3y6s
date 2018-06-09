using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("detail")]
    public class DetailController : BaseController
    {
        public DetailController() { }

        public DetailController(IIdentityContext ioc)
            : base(ioc) { }

        [Route("v1/quote-of-day"), HttpGet]
        public IActionResult QuoteOfDay()
        {
            var task = (MaintainQuotesTask)Tasks.Single(x => x.GetType() == typeof(MaintainQuotesTask));

            return Ok(task.QuoteOfDay);
        }

        [Route("v1/change-email"), HttpPut]
        public async Task<IActionResult> AskChangeEmail(UserChangeEmail model)
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

            else if (await IoC.UserMgmt.GetEmailAsync(user) != user.Email)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentEmail);

            else if (model.NewEmail != model.NewEmailConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidEmailConfirm);

            else
            {
                string token = await IoC.UserMgmt.GenerateEmailConfirmationTokenAsync(user);

                if (IoC.ContextStatus == ContextType.UnitTest)
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

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await IoC.UserMgmt.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                string token = await IoC.UserMgmt.GeneratePasswordResetTokenAsync(user);

                if (IoC.ContextStatus == ContextType.UnitTest)
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

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (await IoC.UserMgmt.GetPhoneNumberAsync(user) != model.CurrentPhoneNumber)
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPhone);

            else if (model.NewPhoneNumber != model.NewPhoneNumberConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPhoneConfirm);

            else
            {
                string token = await IoC.UserMgmt.GenerateChangePhoneNumberTokenAsync(user, model.NewPhoneNumber);

                if (IoC.ContextStatus == ContextType.UnitTest)
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

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            bool current = await IoC.UserMgmt.GetTwoFactorEnabledAsync(user);

            if (current == status)
                return BadRequest(BaseLib.Statics.MsgUserInvalidTwoFactor);

            else
            {
                var result = await IoC.UserMgmt.SetTwoFactorEnabledAsync(user, status);

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

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                var update = new UserFactory<UserUpdate>(model);
                var result = await IoC.UserMgmt.UpdateAsync(update.Devolve());

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update.Evolve());
            }
        }
    }
}
