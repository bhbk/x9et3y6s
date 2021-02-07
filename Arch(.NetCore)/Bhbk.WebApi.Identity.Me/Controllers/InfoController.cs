using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("info")]
    [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
    public class InfoController : BaseController
    {
        [Route("v1"), HttpGet]
        public IActionResult GetV1()
        {
            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<UserV1>(user));
        }

        [Route("v1/msg-of-the-day"), HttpGet]
        public IActionResult GetMOTDV1()
        {
            var random = new Random();
            var skip = random.Next(1, uow.MOTDs.Count());

            if (skip == 1)
                skip = 0;

            var motd = uow.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<uvw_MOTD>()
                .OrderBy("id").Skip(skip).Take(1).ToLambda())
                .SingleOrDefault();

            return Ok(map.Map<MOTDTssV1>(motd));
        }

        [Route("v1/code"), HttpGet]
        public IActionResult GetCodesV1()
        {
            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            var states = uow.States.Get(x => x.UserId == user.Id);

            var result = states.Select(x => map.Map<StateV1>(x));

            return Ok(result);
        }

        [Route("v1/code/revoke"), HttpDelete]
        public IActionResult DeleteCodesV1()
        {
            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            uow.States.Delete(QueryExpressionFactory.GetQueryExpression<uvw_State>()
                .Where(x => x.UserId == user.Id).ToLambda());
            uow.Commit();

            return NoContent();
        }

        [Route("v1/code/{codeID}/revoke"), HttpDelete]
        public IActionResult DeleteCodeV1([FromRoute] Guid codeID)
        {
            var code = uow.States.Get(x => x.UserId == GetIdentityGUID()
                && x.Id == codeID).SingleOrDefault();

            if (code == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            uow.States.Delete(QueryExpressionFactory.GetQueryExpression<uvw_State>()
                .Where(x => x.Id == code.Id).ToLambda());
            uow.Commit();

            return NoContent();
        }

        [Route("v1/code/{codeValue}/{actionValue}"), HttpGet]
        public IActionResult UpdateCodeV1([FromRoute] string codeValue, string actionValue)
        {
            ActionType actionType;

            if (!Enum.TryParse<ActionType>(actionValue, true, out actionType))
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"Action:{actionValue}");
                return BadRequest(ModelState);
            }

            var state = uow.States.Get(x => x.StateValue == codeValue).SingleOrDefault();

            if (state == null)
            {
                ModelState.AddModelError(MessageType.StateNotFound.ToString(), $"Code:{codeValue}");
                return NotFound(ModelState);
            }
            
            if (state.StateDecision.HasValue
                && state.StateDecision.Value == false)
            {
                ModelState.AddModelError(MessageType.StateDenied.ToString(), $"Code:{codeValue}");
                return BadRequest(ModelState);
            }

            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null
                || user.Id != state.UserId)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{state.UserId}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            if(state.UserId != user.Id)
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"Code:{codeValue} User:{user.Id}");
                return BadRequest(ModelState);
            }

            if (string.Equals(actionValue, ActionType.Allow.ToString(), StringComparison.OrdinalIgnoreCase))
                state.StateDecision = true;

            else if (string.Equals(actionValue, ActionType.Deny.ToString(), StringComparison.OrdinalIgnoreCase))
                state.StateDecision = false;

            else
                throw new NotImplementedException();

            uow.States.Update(state);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/refresh"), HttpGet]
        public IActionResult GetRefreshesV1()
        {
            var expr = QueryExpressionFactory.GetQueryExpression<uvw_Refresh>()
                .Where(x => x.UserId == GetIdentityGUID()).ToLambda();

            if (!uow.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            var refreshes = uow.Refreshes.Get(expr);

            return Ok(map.Map<IEnumerable<RefreshV1>>(refreshes));
        }

        [Route("v1/refresh/revoke"), HttpDelete]
        public IActionResult DeleteRefreshesV1()
        {
            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            uow.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Refresh>()
                .Where(x => x.UserId == user.Id).ToLambda());
            uow.Commit();

            return NoContent();
        }

        [Route("v1/refresh/{refreshID}/revoke"), HttpDelete]
        public IActionResult DeleteRefreshV1([FromRoute] Guid refreshID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<uvw_Refresh>()
                .Where(x => x.UserId == GetIdentityGUID() && x.Id == refreshID).ToLambda();

            if (!uow.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            uow.Refreshes.Delete(expr);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/set-password"), HttpPut]
        public IActionResult SetPasswordV1([FromBody] PasswordChangeV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

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

            uow.Users.SetPassword(user, model.NewPassword);
            uow.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public IActionResult UpdateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

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

            var result = uow.Users.Update(map.Map<uvw_User>(model));

            uow.Commit();

            return Ok(map.Map<UserV1>(result));
        }
    }
}
