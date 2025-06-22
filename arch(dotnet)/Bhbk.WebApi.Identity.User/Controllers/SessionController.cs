using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Identity.User.Controllers
{
    [Route("session")]
    [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
    public class SessionController : BaseController
    {
        #region Refresh Tokens

        [Route("v1/refreshes"), HttpGet]
        public IActionResult GetRefreshesV1()
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == GetIdentityGUID()).ToLambda();

            if (!uow.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            var refreshes = uow.Refreshes.Get(expr);

            return Ok(map.Map<IEnumerable<RefreshV1>>(refreshes));
        }

        [Route("v1/refreshes"), HttpDelete]
        public IActionResult DeleteRefreshesV1()
        {
            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            uow.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == user.Id).ToLambda());
            uow.Commit();

            return NoContent();
        }

        [Route("v1/refreshes/{refreshID:guid}"), HttpDelete]
        public IActionResult DeleteRefreshV1([FromRoute] Guid refreshID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
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

        #endregion

        #region Authorization Codes

        [Route("v1/codes"), HttpGet]
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

        [Route("v1/codes"), HttpDelete]
        public IActionResult DeleteCodesV1()
        {
            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            uow.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.UserId == user.Id).ToLambda());
            uow.Commit();

            return NoContent();
        }

        [Route("v1/codes/{codeID:guid}"), HttpDelete]
        public IActionResult DeleteCodeV1([FromRoute] Guid codeID)
        {
            var code = uow.States.Get(x => x.UserId == GetIdentityGUID()
                && x.Id == codeID).SingleOrDefault();

            if (code == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            uow.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.Id == code.Id).ToLambda());
            uow.Commit();

            return NoContent();
        }

        [Route("v1/codes/{codeValue}/{actionValue}"), HttpGet]
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
            /* verify user is confirmed and not locked */
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            if (state.UserId != user.Id)
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

        #endregion

        #region Logout

        [Route("v1/logout"), HttpPost]
        [AllowAnonymous]
        public IActionResult LogoutV1()
        {
            var refreshToken = Request.Cookies["refresh_token"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                    .Where(x => x.RefreshValue == refreshToken).ToLambda()).SingleOrDefault();

                if (refresh != null)
                {
                    uow.Refreshes.Delete(refresh);
                    uow.Commit();
                }
            }

            ClearRefreshTokenCookie();
            return Ok();
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/oauth2" });
        }

        #endregion
    }
}
