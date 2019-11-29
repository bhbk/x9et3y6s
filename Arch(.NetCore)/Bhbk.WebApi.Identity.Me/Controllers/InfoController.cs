﻿using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
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
        public IActionResult GetUserV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<UserModel>(user));
        }

        [Route("v1/msg-of-the-day"), HttpGet]
        [AllowAnonymous]
        public IActionResult GetMOTDV1()
        {
            var random = new Random();
            var skip = random.Next(1, UoW.MOTDs.Count());

            if (skip == 1)
                skip = 0;

            var motd = UoW.MOTDs.Get(new QueryExpression<tbl_MotDType1>()
                .OrderBy("id").Skip(skip).Take(1).ToLambda()).SingleOrDefault();

            return Ok(Mapper.Map<MOTDType1Model>(motd));
        }

        [Route("v1/code"), HttpGet]
        public IActionResult GetUserCodesV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            var states = UoW.States.Get(x => x.UserId == user.Id);

            var result = states.Select(x => Mapper.Map<StateModel>(x));

            return Ok(result);
        }

        [Route("v1/code/revoke"), HttpDelete]
        public IActionResult DeleteUserCodesV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            UoW.States.Delete(new QueryExpression<tbl_States>()
                .Where(x => x.UserId == user.Id).ToLambda());
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/code/{codeID}/revoke"), HttpDelete]
        public IActionResult DeleteUserCodeV1([FromRoute] Guid codeID)
        {
            var code = UoW.States.Get(x => x.UserId == GetUserGUID()
                && x.Id == codeID).SingleOrDefault();

            if (code == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            UoW.States.Delete(new QueryExpression<tbl_States>()
                .Where(x => x.Id == code.Id).ToLambda());
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/code/{codeValue}/{actionValue}"), HttpGet]
        public IActionResult UpdateUserCodeV1([FromRoute] string codeValue, string actionValue)
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

            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null
                || user.Id != state.UserId)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{state.UserId}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user.Id)
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
        public IActionResult GetUserRefreshesV1()
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == GetUserGUID()).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            var refreshes = UoW.Refreshes.Get(expr);

            return Ok(Mapper.Map<IEnumerable<RefreshModel>>(refreshes));
        }

        [Route("v1/refresh/revoke"), HttpDelete]
        public IActionResult DeleteUserRefreshesV1()
        {
            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == user.Id).ToLambda());

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/refresh/{refreshID}/revoke"), HttpDelete]
        public IActionResult DeleteUserRefreshV1([FromRoute] Guid refreshID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == GetUserGUID() && x.Id == refreshID).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{GetUserGUID()}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(expr);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/set-password"), HttpPut]
        public IActionResult SetUserPasswordV1([FromBody] EntityChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

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
            else if (!UoW.Users.VerifyPassword(user, model.CurrentPassword)
                || model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            user.ActorId = GetUserGUID();

            UoW.Users.SetPassword(user, model.NewPassword);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/set-two-factor/{statusValue}"), HttpPut]
        public IActionResult SetTwoFactorV1([FromRoute] bool statusValue)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

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

            UoW.Users.SetTwoFactorEnabled(user, statusValue);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public IActionResult UpdateUserV1([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == GetUserGUID()).SingleOrDefault();

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

            var result = UoW.Users.Update(Mapper.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return Ok(Mapper.Map<UserModel>(result));
        }
    }
}