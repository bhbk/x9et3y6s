using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Providers.Me;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("info")]
    [Authorize(Policy = Constants.DefaultPolicyForHumans)]
    public class InfoController : BaseController
    {
        private InfoProvider _provider;

        public InfoController(IConfiguration conf, IContextService instance)
        {
            _provider = new InfoProvider(conf, instance);
        }

        [Route("v1"), HttpGet]
        public IActionResult GetV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<UserV1>(user));
        }

        [Route("v1/msg-of-the-day"), HttpGet]
        public IActionResult GetMOTDV1()
        {
            var random = new Random();
            var skip = random.Next(1, UoW.MOTDs.Count());

            if (skip == 1)
                skip = 0;

            var motd = UoW.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                .OrderBy("id").Skip(skip).Take(1).ToLambda())
                .SingleOrDefault();

            return Ok(Mapper.Map<MOTDTssV1>(motd));
        }

        [Route("v1/code"), HttpGet]
        public IActionResult GetCodesV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            var states = UoW.States.Get(x => x.UserId == user.Id);

            var result = states.Select(x => Mapper.Map<StateV1>(x));

            return Ok(result);
        }

        [Route("v1/code/revoke"), HttpDelete]
        public IActionResult DeleteCodesV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            UoW.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.UserId == user.Id).ToLambda());
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/code/{codeID}/revoke"), HttpDelete]
        public IActionResult DeleteCodeV1([FromRoute] Guid codeID)
        {
            var code = UoW.States.Get(x => x.UserId == GetIdentityGUID()
                && x.Id == codeID).SingleOrDefault();

            if (code == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            UoW.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.Id == code.Id).ToLambda());
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/code/{codeValue}/{actionValue}"), HttpGet]
        public IActionResult UpdateCodeV1([FromRoute] string codeValue, string actionValue)
        {
            ActionType actionType;

            if (!Enum.TryParse<ActionType>(actionValue, true, out actionType))
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"User action:{actionValue}");
                return BadRequest(ModelState);
            }

            var state = UoW.States.Get(x => x.StateValue == codeValue).SingleOrDefault();

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

            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null
                || user.Id != state.UserId)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{state.UserId}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user)
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

            UoW.States.Update(state);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/refresh"), HttpGet]
        public IActionResult GetRefreshesV1()
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == GetIdentityGUID()).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            var refreshes = UoW.Refreshes.Get(expr);

            return Ok(Mapper.Map<IEnumerable<RefreshV1>>(refreshes));
        }

        [Route("v1/refresh/revoke"), HttpDelete]
        public IActionResult DeleteRefreshesV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == user.Id).ToLambda());

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/refresh/{refreshID}/revoke"), HttpDelete]
        public IActionResult DeleteRefreshV1([FromRoute] Guid refreshID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == GetIdentityGUID() && x.Id == refreshID).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(expr);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/set-password"), HttpPut]
        public IActionResult SetPasswordV1([FromBody] PasswordChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }
            else if (!user.IsHumanBeing)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }
            else if (!new ValidationHelper().ValidatePassword(model.CurrentPassword).Succeeded
                || !new ValidationHelper().ValidatePassword(model.NewPassword).Succeeded
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            user.ActorId = GetIdentityGUID();

            UoW.Users.SetPasswordHash(user, model.NewPassword);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/set-multi-factor/{statusValue}"), HttpPut]
        public IActionResult SetTwoFactorV1([FromRoute] bool statusValue)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }
            else if (!user.IsHumanBeing
                || user.IsMultiFactor == statusValue)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetMultiFactorEnabled(user, statusValue);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public IActionResult UpdateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            if (user.Id != model.Id
                || !user.IsHumanBeing)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var result = UoW.Users.Update(Mapper.Map<tbl_User>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return Ok(Mapper.Map<UserV1>(result));
        }
    }
}
