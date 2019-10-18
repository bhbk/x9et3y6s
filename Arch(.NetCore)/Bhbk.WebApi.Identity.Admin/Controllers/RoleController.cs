using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
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
using System.Threading.Tasks;

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
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> CreateRoleV1([FromBody] RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.Roles.GetAsync(x => x.ClientId == model.ClientId
                && x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MessageType.RoleAlreadyExists.ToString(), $"Client:{model.ClientId} Role:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var create = await UoW.Roles.CreateAsync(Mapper.Map<tbl_Roles>(model));

            await UoW.CommitAsync();

            var result = (await UoW.Roles.GetAsync(x => x.Name == model.Name)).SingleOrDefault();

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(Mapper.Map<RoleModel>(result));
        }

        [Route("v1/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteRoleV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.Roles.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"roleID: { roleID }");
                return NotFound(ModelState);
            }
            else if (role.Immutable)
            {
                ModelState.AddModelError(MessageType.RoleImmutable.ToString(), $"Role:{role.Id}");
                return BadRequest(ModelState);
            }

            role.ActorId = GetUserGUID();

            await UoW.Roles.DeleteAsync(role);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{roleValue}"), HttpGet]
        public async Task<IActionResult> GetRoleV1([FromRoute] string roleValue)
        {
            Guid roleID;
            tbl_Roles role = null;

            if (Guid.TryParse(roleValue, out roleID))
                role = (await UoW.Roles.GetAsync(x => x.Id == roleID)).SingleOrDefault();
            else
                role = (await UoW.Roles.GetAsync(x => x.Name == roleValue)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<RoleModel>(role));
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetRolesV1([FromBody] PageState model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateResult<RoleModel>
                {
                    Data = Mapper.Map<IEnumerable<RoleModel>>(
                        await UoW.Roles.GetAsync(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Roles>, IQueryable<tbl_Roles>>>>(
                                model.ToExpression<tbl_Roles>()))),

                    Total = await UoW.Roles.CountAsync(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Roles>, IQueryable<tbl_Roles>>>>(
                            model.ToPredicateExpression<tbl_Roles>()))
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
        public async Task<IActionResult> GetRoleUsersV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.Roles.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var users = await UoW.Users.GetAsync(new QueryExpression<tbl_Users>()
                .Where(x => x.tbl_UserRoles.Any(y => y.RoleId == roleID)).ToLambda());

            return Ok(Mapper.Map<UserModel>(users));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> UpdateRoleV1([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.Roles.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{model.Id}");
                return NotFound(ModelState);
            }
            else if (role.Immutable
                && role.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.RoleImmutable.ToString(), $"Role:{role.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.Roles.UpdateAsync(Mapper.Map<tbl_Roles>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<RoleModel>(result));
        }
    }
}
