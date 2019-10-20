using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Me;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("info")]
    public class InfoController : BaseController
    {
        private InfoProvider _provider;

        public InfoController(IConfiguration conf, IContextService instance)
        {
            _provider = new InfoProvider(conf, instance);
        }

        [Route("v1"), HttpGet]
        public async ValueTask<IActionResult> GetUserV1()
        {
            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<UserModel>(user));
        }

        [Route("v1/msg-of-the-day"), HttpGet]
        [AllowAnonymous]
        public async ValueTask<IActionResult> GetMOTDV1()
        {
            var random = new Random();
            var skip = random.Next(1, await UoW.MOTDs.CountAsync());

            if (skip == 1)
                skip = 0;

            var motd = (await UoW.MOTDs.GetAsync(
                new QueryExpression<tbl_MotDType1>().OrderBy("id").Skip(skip).Take(1).ToLambda())).SingleOrDefault();

            return Ok(Mapper.Map<MOTDType1Model>(motd));
        }

        [Route("v1/code"), HttpGet]
        public async ValueTask<IActionResult> GetUserCodesV1()
        {
            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            var states = await UoW.States.GetAsync(x => x.UserId == user.Id);

            var result = states.Select(x => Mapper.Map<StateModel>(x));

            return Ok(result);
        }

        [Route("v1/code/revoke"), HttpDelete]
        public async ValueTask<IActionResult> DeleteUserCodesV1()
        {
            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            await UoW.States.DeleteAsync(new QueryExpression<tbl_States>()
                .Where(x => x.UserId == user.Id).ToLambda());
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/code/{codeID}/revoke"), HttpDelete]
        public async ValueTask<IActionResult> DeleteUserCodeV1([FromRoute] Guid codeID)
        {
            var code = (await UoW.States.GetAsync(x => x.UserId == GetUserGUID()
                && x.Id == codeID)).SingleOrDefault();

            if (code == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            await UoW.States.DeleteAsync(new QueryExpression<tbl_States>()
                .Where(x => x.Id == code.Id).ToLambda());
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/code/{codeValue}/{actionValue}"), HttpGet]
        public async ValueTask<IActionResult> UpdateUserCodeV1([FromRoute] string codeValue, string actionValue)
        {
            ActionType actionType;

            if (!Enum.TryParse<ActionType>(actionValue, true, out actionType))
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"User action:{actionValue}");
                return BadRequest(ModelState);
            }

            var state = (await UoW.States.GetAsync(x => x.StateValue == codeValue)).SingleOrDefault();

            if (state == null)
            {
                ModelState.AddModelError(MessageType.StateNotFound.ToString(), $"User code:{codeValue}");
                return NotFound(ModelState);
            }
            else if (state.StateDecision.HasValue
                && state.StateDecision.Value == false)
            {
                ModelState.AddModelError(MessageType.StateDenied.ToString(), $"User code:{codeValue}");
                return BadRequest(ModelState);
            }

            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null
                || user.Id != state.UserId)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{state.UserId}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.Users.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            if (string.Equals(actionValue, ActionType.Allow.ToString(), StringComparison.OrdinalIgnoreCase))
                state.StateDecision = true;
            else if (string.Equals(actionValue, ActionType.Deny.ToString(), StringComparison.OrdinalIgnoreCase))
                state.StateDecision = false;
            else
                throw new NotImplementedException();

            await UoW.States.UpdateAsync(state);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/refresh"), HttpGet]
        public async ValueTask<IActionResult> GetUserRefreshesV1()
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == GetUserGUID()).ToLambda();

            if (!await UoW.Refreshes.ExistsAsync(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            var refreshes = await UoW.Refreshes.GetAsync(expr);

            return Ok(Mapper.Map<IEnumerable<RefreshModel>>(refreshes));
        }

        [Route("v1/refresh/revoke"), HttpDelete]
        public async ValueTask<IActionResult> DeleteUserRefreshesV1()
        {
            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            await UoW.Refreshes.DeleteAsync(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == user.Id).ToLambda());

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/refresh/{refreshID}/revoke"), HttpDelete]
        public async ValueTask<IActionResult> DeleteUserRefreshV1([FromRoute] Guid refreshID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == GetUserGUID() && x.Id == refreshID).ToLambda();

            if (!await UoW.Refreshes.ExistsAsync(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            await UoW.Refreshes.DeleteAsync(expr);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/set-password"), HttpPut]
        public async ValueTask<IActionResult> SetUserPasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }
            else if (!user.HumanBeing)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }
            else if (!await UoW.Users.VerifyPasswordAsync(user.Id, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            user.ActorId = GetUserGUID();

            await UoW.Users.SetPasswordAsync(user, model.NewPassword);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/set-two-factor/{statusValue}"), HttpPut]
        public async ValueTask<IActionResult> SetTwoFactorV1([FromRoute] bool statusValue)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }
            else if (!user.HumanBeing
                || user.TwoFactorEnabled == statusValue)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            await UoW.Users.SetTwoFactorEnabledAsync(user, statusValue);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public async ValueTask<IActionResult> UpdateUserV1([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.Users.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            if (user.Id != model.Id
                || !user.HumanBeing)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var result = await UoW.Users.UpdateAsync(Mapper.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return Ok(Mapper.Map<UserModel>(result));
        }
    }
}
