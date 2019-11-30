using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
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
        private LoginProvider _provider;

        public LoginController(IConfiguration conf, IContextService instance)
        {
            _provider = new LoginProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult CreateV1([FromBody] LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Logins.Get(x => x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.LoginAlreadyExists.ToString(), $"Login:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var result = UoW.Logins.Create(Mapper.Map<tbl_Logins>(model));

            UoW.Commit();

            return Ok(Mapper.Map<LoginModel>(result));
        }

        [Route("v1/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteV1([FromRoute] Guid loginID)
        {
            var login = UoW.Logins.Get(x => x.Id == loginID).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login: { loginID }");
                return NotFound(ModelState);
            }
            else if (login.Immutable)
            {
                ModelState.AddModelError(MessageType.LoginImmutable.ToString(), $"Login:{login.Id}");
                return BadRequest(ModelState);
            }

            login.ActorId = GetIdentityGUID();

            UoW.Logins.Delete(login);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{loginValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string loginValue)
        {
            Guid loginID;
            tbl_Logins login = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(loginValue, out loginID))
                login = UoW.Logins.Get(x => x.Id == loginID).SingleOrDefault();
            else
                login = UoW.Logins.Get(x => x.Name == loginValue).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<LoginModel>(login));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<LoginModel>
                {
                    Data = Mapper.Map<IEnumerable<LoginModel>>(
                        UoW.Logins.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Logins>, IQueryable<tbl_Logins>>>>(
                                model.ToExpression<tbl_Logins>()),
                            new List<Expression<Func<tbl_Logins, object>>>() { x => x.tbl_UserLogins })),

                    Total = UoW.Logins.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Logins>, IQueryable<tbl_Logins>>>>(
                            model.ToPredicateExpression<tbl_Logins>()))
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
            var login = UoW.Logins.Get(x => x.Id == loginID).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            var users = UoW.Users.Get(new QueryExpression<tbl_Users>()
                .Where(x => x.tbl_UserLogins.Any(y => y.LoginId == loginID)).ToLambda());

            return Ok(Mapper.Map<UserModel>(users));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult UpdateV1([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = UoW.Logins.Get(x => x.Id == model.Id).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{model.Id}");
                return NotFound(ModelState);
            }
            else if (login.Immutable
                && login.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.LoginImmutable.ToString(), $"Login:{login.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var result = UoW.Logins.Update(Mapper.Map<tbl_Logins>(model));

            UoW.Commit();

            return Ok(Mapper.Map<LoginModel>(result));
        }
    }
}