using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("role")]
    public class RoleController : BaseController
    {
        public RoleController() { }

        public RoleController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1/{roleID}/add/{userID}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddRoleToUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                var result = await IoC.UserMgmt.AddToRoleAsync(user, role.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var role = await IoC.RoleMgmt.FindByNameAsync(model.Name);

            if (role != null)
                return BadRequest(BaseLib.Statics.MsgRoleAlreadyExists);

            var create = new RoleFactory<RoleCreate>(model);

            var result = await IoC.RoleMgmt.CreateAsync(create.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            else
                return Ok(create.Evolve());
        }

        [Route("v1/{roleID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteRole(Guid roleID)
        {
            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                role.ActorId = GetUserGUID();

                var result = await IoC.RoleMgmt.DeleteAsync(role);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetRoles([FromQuery] CustomPagingModel filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roles = IoC.RoleMgmt.Store.Get().AsQueryable()
                .OrderBy(filter.OrderBy)
                .Skip(Convert.ToInt32((filter.PageNumber - 1) * filter.PageSize))
                .Take(Convert.ToInt32(filter.PageSize));

            var result = roles.Select(x => new RoleFactory<AppRole>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1/{roleID}"), HttpGet]
        public async Task<IActionResult> GetRole(Guid roleID)
        {
            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var result = new RoleFactory<AppRole>(role);

            return Ok(result.Evolve());
        }

        [Route("v1/{roleID}/users"), HttpGet]
        public async Task<IActionResult> GetRoleUsers(Guid roleID)
        {
            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var users = await IoC.RoleMgmt.GetUsersListAsync(role);

            var result = users.Select(x => new UserFactory<AppUser>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1/{roleID}/remove/{userID}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveRoleFromUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                var result = await IoC.UserMgmt.RemoveFromRoleAsync(user, role.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var role = await IoC.RoleMgmt.FindByIdAsync(model.Id.ToString());

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                var update = new RoleFactory<AppRole>(role);
                update.Update(model);

                var result = await IoC.RoleMgmt.UpdateAsync(update.Devolve());

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update.Evolve());
            }
        }
    }
}
