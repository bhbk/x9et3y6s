using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
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
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            return Ok(UoW.Transform.Map<UserModel>(user));
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

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, model.CurrentPassword))
                return BadRequest(Strings.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            user.ActorId = GetUserGUID();

            if (!await UoW.UserRepo.RemovePasswordAsync(user.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!await UoW.UserRepo.AddPasswordAsync(user.Id, model.NewPassword))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/two-factor/{status}"), HttpPut]
        public async Task<IActionResult> SetTwoFactorV1([FromRoute] bool status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            if (user.TwoFactorEnabled == status)
                return BadRequest(Strings.MsgUserInvalidTwoFactor);

            if (!await UoW.UserRepo.SetTwoFactorEnabledAsync(user.Id, status))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public async Task<IActionResult> UpdateDetailV1([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (user.Id != model.Id)
                return BadRequest(Strings.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            var result = await UoW.UserRepo.UpdateAsync(UoW.Transform.Map<AppUser>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<UserModel>(result));
        }
    }
}
