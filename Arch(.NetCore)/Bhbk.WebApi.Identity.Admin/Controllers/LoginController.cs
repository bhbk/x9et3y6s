using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
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
using System.Threading.Tasks;

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
        public async ValueTask<IActionResult> CreateLoginV1([FromBody] LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.Logins.GetAsync(x => x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MessageType.LoginAlreadyExists.ToString(), $"Login:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.Logins.CreateAsync(Mapper.Map<tbl_Logins>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<LoginModel>(result));
        }

        [Route("v1/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> DeleteLoginV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.Logins.GetAsync(x => x.Id == loginID)).SingleOrDefault();

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

            login.ActorId = GetUserGUID();

            await UoW.Logins.DeleteAsync(login);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{loginValue}"), HttpGet]
        public async ValueTask<IActionResult> GetLoginV1([FromRoute] string loginValue)
        {
            Guid loginID;
            tbl_Logins login = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(loginValue, out loginID))
                login = (await UoW.Logins.GetAsync(x => x.Id == loginID)).SingleOrDefault();
            else
                login = (await UoW.Logins.GetAsync(x => x.Name == loginValue)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<LoginModel>(login));
        }

        [Route("v1/page"), HttpPost]
        public async ValueTask<IActionResult> GetLoginsV1([FromBody] PageState model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateResult<LoginModel>
                {
                    Data = Mapper.Map<IEnumerable<LoginModel>>(
                        await UoW.Logins.GetAsync(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Logins>, IQueryable<tbl_Logins>>>>(
                                model.ToExpression<tbl_Logins>()))),

                    Total = await UoW.Logins.CountAsync(
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
        public async ValueTask<IActionResult> GetLoginUsersV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.Logins.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            var users = await UoW.Users.GetAsync(new QueryExpression<tbl_Users>()
                .Where(x => x.tbl_UserLogins.Any(y => y.LoginId == loginID)).ToLambda());

            return Ok(Mapper.Map<UserModel>(users));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> UpdateLoginV1([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = (await UoW.Logins.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{model.Id}");
                return NotFound(ModelState);
            }
            else if (login.Immutable
                && login.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.LoginImmutable.ToString(), $"Client:{login.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.Logins.UpdateAsync(Mapper.Map<tbl_Logins>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<LoginModel>(result));
        }
    }
}