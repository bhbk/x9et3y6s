using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
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
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            return Ok(UoW.Shape.Map<UserModel>(user));
        }

        [Route("v1/quotes"), HttpGet]
        public IActionResult GetQuoteOfTheDayV1()
        {
            var task = (MaintainQuotesTask)Tasks.Single(x => x.GetType() == typeof(MaintainQuotesTask));

            return Ok(task.Qotd);
        }

        [Route("v1/set-password"), HttpPut]
        public async Task<IActionResult> SetPasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }
            else if (!user.HumanBeing)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }
            else if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

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
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }
            else if (!user.HumanBeing
                || user.TwoFactorEnabled == status)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

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
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            if (user.Id != model.Id
                || !user.HumanBeing)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var result = await UoW.UserRepo.UpdateAsync(UoW.Shape.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return Ok(UoW.Shape.Map<UserModel>(result));
        }
    }
}
