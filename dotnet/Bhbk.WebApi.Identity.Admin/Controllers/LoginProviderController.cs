using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EF.Models;
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
    [Route("login-providers")]
    public class LoginProviderController : BaseController
    {
        [Route("v1"), HttpPost]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult CreateV1([FromBody] LoginProviderV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (uow.LoginProviders.Get(x => x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.LoginProviderAlreadyExists.ToString(), $"LoginProvider:{model.Name}");
                return BadRequest(ModelState);
            }

            var result = uow.LoginProviders.Post(map.Map<tbl_LoginProvider>(model));

            uow.Commit();

            return Ok(map.Map<LoginProviderV1>(result));
        }

        [Route("v1/{loginProviderID:guid}"), HttpDelete]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult DeleteV1([FromRoute] Guid loginProviderID)
        {
            var loginProvider = uow.LoginProviders.Get(x => x.Id == loginProviderID)
                .SingleOrDefault();

            if (loginProvider == null)
            {
                ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"LoginProvider: { loginProviderID }");
                return NotFound(ModelState);
            }

            if (!loginProvider.IsDeletable)
            {
                ModelState.AddModelError(MessageType.LoginProviderImmutable.ToString(), $"LoginProvider:{loginProvider.Id}");
                return BadRequest(ModelState);
            }

            uow.LoginProviders.Delete(loginProvider);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{loginProviderValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string loginProviderValue)
        {
            Guid loginProviderID;
            tbl_LoginProvider loginProvider = null;

            /* resolve identifier to guid */
            if (Guid.TryParse(loginProviderValue, out loginProviderID))
                loginProvider = uow.LoginProviders.Get(x => x.Id == loginProviderID)
                    .SingleOrDefault();
            else
                loginProvider = uow.LoginProviders.Get(x => x.Name == loginProviderValue)
                    .SingleOrDefault();

            if (loginProvider == null)
            {
                ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"LoginProvider:{loginProviderValue}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<LoginProviderV1>(loginProvider));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] PagerState state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PagerStateResult<LoginProviderV1>
                {
                    Data = map.Map<IEnumerable<LoginProviderV1>>(
                        uow.LoginProviders.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_LoginProvider>, IQueryable<tbl_LoginProvider>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>().ApplyState(state)),
                                    new List<Expression<Func<tbl_LoginProvider, object>>>()
                                    {
                                        x => x.tbl_UserLoginProviders,
                                    })),

                    Total = uow.LoginProviders.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_LoginProvider>, IQueryable<tbl_LoginProvider>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{loginProviderID:guid}/users"), HttpGet]
        public IActionResult GetUsersV1([FromRoute] Guid loginProviderID)
        {
            var loginProvider = uow.LoginProviders.Get(x => x.Id == loginProviderID)
                .SingleOrDefault();

            if (loginProvider == null)
            {
                ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"LoginProvider:{loginProviderID}");
                return NotFound(ModelState);
            }

            var users = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.tbl_UserLoginProviders.Any(y => y.LoginProviderId == loginProviderID)).ToLambda());

            return Ok(map.Map<UserV1>(users));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult UpdateV1([FromBody] LoginProviderV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loginProvider = uow.LoginProviders.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (loginProvider == null)
            {
                ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"LoginProvider:{model.Id}");
                return NotFound(ModelState);
            }
            else if (loginProvider.IsDeletable
                && loginProvider.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.LoginProviderImmutable.ToString(), $"LoginProvider:{loginProvider.Id}");
                return BadRequest(ModelState);
            }

            /* preserve existing provider key when masked value is returned unchanged */
            if (model.ProviderKey == "********")
                model.ProviderKey = loginProvider.ProviderKey;

            var result = uow.LoginProviders.Put(map.Map<tbl_LoginProvider>(model));

            uow.Commit();

            return Ok(map.Map<LoginProviderV1>(result));
        }
    }
}
