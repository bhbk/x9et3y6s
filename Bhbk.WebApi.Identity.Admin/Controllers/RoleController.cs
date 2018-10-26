﻿using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("role")]
    public class RoleController : BaseController
    {
        public RoleController() { }

        public RoleController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1/{roleID:guid}/add/{userID:guid}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddRoleToUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await UoW.CustomRoleMgr.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.CustomUserMgr.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateRoleV1([FromBody] RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.CustomRoleMgr.FindByNameAsync(model.Name);

            if (check != null)
                return BadRequest(Strings.MsgRoleAlreadyExists);

            var create = await UoW.CustomRoleMgr.CreateAsync(UoW.Maps.Map<AppRole>(model));

            if (!create.Succeeded)
                return GetErrorResult(create);

            await UoW.CommitAsync();

            var result = await UoW.CustomRoleMgr.FindByNameAsync(model.Name);

            if(result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(UoW.Maps.Map<RoleResult>(result));
        }

        [Route("v1/{roleID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteRoleV1([FromRoute] Guid roleID)
        {
            var role = await UoW.CustomRoleMgr.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(Strings.MsgRoleImmutable);

            role.ActorId = GetUserGUID();

            var result = await UoW.CustomRoleMgr.DeleteAsync(role);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpGet]
        public IActionResult GetRolesV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roles = UoW.CustomRoleMgr.Store.Get()
                .OrderBy(model.OrderBy)
                .Skip(model.Skip)
                .Take(model.Take);

            var result = roles.Select(x => UoW.Maps.Map<RoleResult>(x));

            return Ok(result);
        }

        [Route("v1/{roleID:guid}"), HttpGet]
        public async Task<IActionResult> GetRoleV1([FromRoute] Guid roleID)
        {
            var role = await UoW.CustomRoleMgr.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            return Ok(UoW.Maps.Map<RoleResult>(role));
        }

        [Route("v1/{roleName}"), HttpGet]
        public async Task<IActionResult> GetRoleV1([FromRoute] string roleName)
        {
            var role = await UoW.CustomRoleMgr.FindByNameAsync(roleName.ToString());

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            return Ok(UoW.Maps.Map<RoleResult>(role));
        }

        [Route("v1/{roleID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetRoleUsersV1([FromRoute] Guid roleID)
        {
            var role = await UoW.CustomRoleMgr.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            var users = UoW.CustomRoleMgr.GetUsersListAsync(role);

            var result = users.Select(x => UoW.Maps.Map<UserResult>(x));

            return Ok(result);
        }

        [Route("v1/{roleID:guid}/remove/{userID:guid}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveRoleFromUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await UoW.CustomRoleMgr.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.CustomUserMgr.RemoveFromRoleAsync(user, role.Name);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateRoleV1([FromBody] RoleUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var role = await UoW.CustomRoleMgr.FindByIdAsync(model.Id.ToString());

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(Strings.MsgRoleImmutable);

            var create = await UoW.CustomRoleMgr.UpdateAsync(UoW.Maps.Map<AppRole>(model));

            if (!create.Succeeded)
                return GetErrorResult(create);

            await UoW.CommitAsync();

            var result = await UoW.CustomRoleMgr.FindByNameAsync(model.Name);

            return Ok(UoW.Maps.Map<RoleResult>(result));
        }
    }
}
