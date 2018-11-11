using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("detail")]
    public class DetailController : BaseController
    {
        public DetailController() { }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetDetailV1()
        {
            var user = await UoW.UserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            return Ok(UoW.Convert.Map<UserResult>(user));
        }

        [Route("v1/quotes"), HttpGet]
        public IActionResult GetQuoteOfTheDayV1()
        {
            var task = (MaintainQuotesTask)Tasks.Single(x => x.GetType() == typeof(MaintainQuotesTask));

            return Ok(task.QuoteOfDay);
        }

        [Route("v1/set-password"), HttpPut]
        public async Task<IActionResult> SetPasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!await UoW.UserMgr.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(Strings.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            user.ActorId = GetUserGUID();

            var remove = await UoW.UserMgr.RemovePasswordAsync(user);

            if (!remove.Succeeded)
                return GetErrorResult(remove);

            var add = await UoW.UserMgr.AddPasswordAsync(user, model.NewPassword);

            if (!add.Succeeded)
                return GetErrorResult(add);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/two-factor/{status}"), HttpPut]
        public async Task<IActionResult> SetTwoFactorV1([FromRoute] bool status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            bool two = await UoW.UserMgr.GetTwoFactorEnabledAsync(user);

            if (two == status)
                return BadRequest(Strings.MsgUserInvalidTwoFactor);

            var result = await UoW.UserMgr.SetTwoFactorEnabledAsync(user, status);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public async Task<IActionResult> UpdateDetailV1([FromBody] UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (user.Id != model.Id)
                return BadRequest(Strings.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            var update = await UoW.UserMgr.UpdateAsync(UoW.Convert.Map<AppUser>(model));

            if (!update.Succeeded)
                return GetErrorResult(update);

            await UoW.CommitAsync();

            var result = await UoW.UserMgr.FindByIdAsync(model.Id.ToString());

            if(result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(UoW.Convert.Map<UserResult>(result));
        }
    }
}
