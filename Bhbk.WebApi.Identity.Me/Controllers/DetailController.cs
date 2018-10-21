using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        public DetailController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/quote-of-day"), HttpGet]
        public IActionResult GetQuoteOfDayV1()
        {
            var task = (MaintainQuotesTask)Tasks.Single(x => x.GetType() == typeof(MaintainQuotesTask));

            return Ok(task.QuoteOfDay);
        }

        [Route("v1/claims"), HttpGet]
        public IActionResult GetClaimsV1()
        {
            var user = User.Identity as ClaimsIdentity;

            return Ok(user.Claims);
        }

        [Route("v1/set-password"), HttpPut]
        public async Task<IActionResult> SetPasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            else if (!await IoC.UserMgmt.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            if (!user.HumanBeing)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            user.ActorId = GetUserGUID();

            var remove = await IoC.UserMgmt.RemovePasswordAsync(user);

            if (!remove.Succeeded)
                return GetErrorResult(remove);

            var add = await IoC.UserMgmt.AddPasswordAsync(user, model.NewPassword);

            if (!add.Succeeded)
                return GetErrorResult(add);

            return NoContent();
        }

        [Route("v1/two-factor/{status}"), HttpPut]
        public async Task<IActionResult> SetTwoFactorV1([FromRoute] bool status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            else if (!user.HumanBeing)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            bool two = await IoC.UserMgmt.GetTwoFactorEnabledAsync(user);

            if (two == status)
                return BadRequest(BaseLib.Statics.MsgUserInvalidTwoFactor);

            var result = await IoC.UserMgmt.SetTwoFactorEnabledAsync(user, status);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public async Task<IActionResult> UpdateDetailV1([FromBody] UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            if (user.Id != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var update = new UserFactory<AppUser>(user);
            update.Update(model);

            var result = await IoC.UserMgmt.UpdateAsync(update.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok(update.Evolve());
        }
    }
}
