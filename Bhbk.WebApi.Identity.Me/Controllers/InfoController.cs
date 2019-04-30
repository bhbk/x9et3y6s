using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("info")]
    public class InfoController : BaseController
    {
        public InfoController() { }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetUserV1()
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            return Ok(UoW.Mapper.Map<UserModel>(user));
        }

        [Route("v1/msg-of-the-day"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetMOTDV1()
        {
            var blah = await UoW.UserRepo.CountMOTDAsync();

            var random = new Random();
            var skip = random.Next(1, await UoW.UserRepo.CountMOTDAsync());

            var quote = (await UoW.UserRepo.GetMOTDAsync(null, null, x => x.OrderBy(y => y.Id), skip, 1)).SingleOrDefault();

            return Ok(UoW.Mapper.Map<MotDType1Model>(quote));
        }

        [Route("v1/code"), HttpGet]
        public async Task<IActionResult> GetUserCodesV1()
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            var states = await UoW.StateRepo.GetAsync(x => x.UserId == user.Id);

            var result = states.Select(x => UoW.Mapper.Map<StateModel>(x));

            return Ok(result);
        }

        [Route("v1/code/revoke"), HttpDelete]
        public async Task<IActionResult> DeleteUserCodesV1()
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            foreach (var token in await UoW.StateRepo.GetAsync(x => x.UserId == user.Id))
            {
                if (!await UoW.StateRepo.DeleteAsync(token.Id))
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/code/{codeID}/revoke"), HttpDelete]
        public async Task<IActionResult> DeleteUserCodeV1([FromRoute] Guid codeID)
        {
            var code = (await UoW.StateRepo.GetAsync(x => x.UserId == GetUserGUID()
                && x.Id == codeID)).SingleOrDefault();

            if (code == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            if (!await UoW.StateRepo.DeleteAsync(code.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/code/{codeValue}/{actionValue}"), HttpGet]
        public async Task<IActionResult> UpdateUserCodeV1([FromRoute] string codeValue, string actionValue)
        {
            ActionType actionType;

            if (!Enum.TryParse<ActionType>(actionValue, true, out actionType))
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"User action:{actionValue}");
                return BadRequest(ModelState);
            }

            var state = (await UoW.StateRepo.GetAsync(x => x.StateValue == codeValue)).SingleOrDefault();

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

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null
                || user.Id != state.UserId)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{state.UserId}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
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

            await UoW.StateRepo.UpdateAsync(state);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/refresh"), HttpGet]
        public async Task<IActionResult> GetUserRefreshesV1()
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            var tokens = await UoW.RefreshRepo.GetAsync(x => x.UserId == user.Id);

            var result = tokens.Select(x => UoW.Mapper.Map<RefreshModel>(x));

            return Ok(result);
        }

        [Route("v1/refresh/revoke"), HttpDelete]
        public async Task<IActionResult> DeleteUserRefreshesV1()
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            foreach (var token in await UoW.RefreshRepo.GetAsync(x => x.UserId == user.Id))
            {
                if (!await UoW.RefreshRepo.DeleteAsync(token.Id))
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/refresh/{refreshID}/revoke"), HttpDelete]
        public async Task<IActionResult> DeleteUserRefreshV1([FromRoute] Guid refreshID)
        {
            var token = (await UoW.RefreshRepo.GetAsync(x => x.UserId == GetUserGUID()
                && x.Id == refreshID)).SingleOrDefault();

            if (token == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            if (!await UoW.RefreshRepo.DeleteAsync(token.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/set-password"), HttpPut]
        public async Task<IActionResult> SetPasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

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
            else if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
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

        [Route("v1/set-two-factor/{statusValue}"), HttpPut]
        public async Task<IActionResult> SetTwoFactorV1([FromRoute] bool statusValue)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

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

            if (!await UoW.UserRepo.SetTwoFactorEnabledAsync(user.Id, statusValue))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public async Task<IActionResult> UpdateUserV1([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == GetUserGUID())).SingleOrDefault();

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

            var result = await UoW.UserRepo.UpdateAsync(UoW.Mapper.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return Ok(UoW.Mapper.Map<UserModel>(result));
        }
    }
}
