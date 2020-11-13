using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
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
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("role")]
    public class RoleController : BaseController
    {
        private RoleProvider _provider;

        public RoleController(IConfiguration conf, IContextService instance)
        {
            _provider = new RoleProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult CreateV1([FromBody] RoleV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Roles.Get(x => x.AudienceId == model.AudienceId
                && x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.RoleAlreadyExists.ToString(), $"Audience:{model.AudienceId} Role:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var create = UoW.Roles.Create(Mapper.Map<tbl_Role>(model));

            UoW.Commit();

            var result = UoW.Roles.Get(x => x.Name == model.Name)
                .SingleOrDefault();

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(Mapper.Map<RoleV1>(result));
        }

        [Route("v1/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid roleID)
        {
            var role = UoW.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"roleID: { roleID }");
                return NotFound(ModelState);
            }
            else if (role.IsDeletable)
            {
                ModelState.AddModelError(MessageType.RoleImmutable.ToString(), $"Role:{role.Id}");
                return BadRequest(ModelState);
            }

            role.ActorId = GetIdentityGUID();

            UoW.Roles.Delete(role);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{roleValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string roleValue)
        {
            Guid roleID;
            tbl_Role role = null;

            if (Guid.TryParse(roleValue, out roleID))
                role = UoW.Roles.Get(x => x.Id == roleID)
                    .SingleOrDefault();
            else
                role = UoW.Roles.Get(x => x.Name == roleValue)
                    .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<RoleV1>(role));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<RoleV1>
                {
                    Data = Mapper.Map<IEnumerable<RoleV1>>(
                        UoW.Roles.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Role>, IQueryable<tbl_Role>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Role>().ApplyState(state)),
                            new List<Expression<Func<tbl_Role, object>>>() { x => x.tbl_UserRoles })),

                    Total = UoW.Roles.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Role>, IQueryable<tbl_Role>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Role>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{roleID:guid}/users"), HttpGet]
        public IActionResult GetUsersV1([FromRoute] Guid roleID)
        {
            var role = UoW.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var users = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.tbl_UserRoles.Any(y => y.RoleId == roleID)).ToLambda());

            return Ok(Mapper.Map<UserV1>(users));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult UpdateV1([FromBody] RoleV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = UoW.Roles.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{model.Id}");
                return NotFound(ModelState);
            }
            else if (role.IsDeletable
                && role.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.RoleImmutable.ToString(), $"Role:{role.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var result = UoW.Roles.Update(Mapper.Map<tbl_Role>(model));

            UoW.Commit();

            return Ok(Mapper.Map<RoleV1>(result));
        }
    }
}
