using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_TBL;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        [Route("v1"), HttpPost]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult CreateV1([FromBody] LoginV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Logins.Get(x => x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.LoginAlreadyExists.ToString(), $"Login:{model.Name}");
                return BadRequest(ModelState);
            }

            var result = UoW.Logins.Create(Mapper.Map<tbl_Login>(model));

            UoW.Commit();

            return Ok(Mapper.Map<LoginV1>(result));
        }

        [Route("v1/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid loginID)
        {
            var login = UoW.Logins.Get(x => x.Id == loginID)
                .SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login: { loginID }");
                return NotFound(ModelState);
            }
            
            if (!login.IsDeletable)
            {
                ModelState.AddModelError(MessageType.LoginImmutable.ToString(), $"Login:{login.Id}");
                return BadRequest(ModelState);
            }

            UoW.Logins.Delete(login);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{loginValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string loginValue)
        {
            Guid loginID;
            tbl_Login login = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(loginValue, out loginID))
                login = UoW.Logins.Get(x => x.Id == loginID)
                    .SingleOrDefault();
            else
                login = UoW.Logins.Get(x => x.Name == loginValue)
                    .SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<LoginV1>(login));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<LoginV1>
                {
                    Data = Mapper.Map<IEnumerable<LoginV1>>(
                        UoW.Logins.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Login>, IQueryable<tbl_Login>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Login>().ApplyState(state)),
                            new List<Expression<Func<tbl_Login, object>>>() { x => x.tbl_UserLogins })),

                    Total = UoW.Logins.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Login>, IQueryable<tbl_Login>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Login>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{loginID:guid}/users"), HttpGet]
        public IActionResult GetUsersV1([FromRoute] Guid loginID)
        {
            var login = UoW.Logins.Get(x => x.Id == loginID)
                .SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            var users = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.tbl_UserLogins.Any(y => y.LoginId == loginID)).ToLambda());

            return Ok(Mapper.Map<UserV1>(users));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult UpdateV1([FromBody] LoginV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = UoW.Logins.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{model.Id}");
                return NotFound(ModelState);
            }
            else if (login.IsDeletable
                && login.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.LoginImmutable.ToString(), $"Login:{login.Id}");
                return BadRequest(ModelState);
            }

            var result = UoW.Logins.Update(Mapper.Map<tbl_Login>(model));

            UoW.Commit();

            return Ok(Mapper.Map<LoginV1>(result));
        }
    }
}