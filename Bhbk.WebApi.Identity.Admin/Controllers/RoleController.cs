using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
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

        public RoleController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/{roleID:guid}/add/{userID:guid}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddRoleToUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(BaseLib.Statics.MsgRoleNotExist);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var result = await IoC.UserMgmt.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateRoleV1([FromBody] RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await IoC.RoleMgmt.FindByNameAsync(model.Name);

            if (check != null)
                return BadRequest(BaseLib.Statics.MsgRoleAlreadyExists);

            var role = new RoleFactory<RoleCreate>(model);

            var result = await IoC.RoleMgmt.CreateAsync(role.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok(role.Evolve());
        }

        [Route("v1/{roleID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteRoleV1([FromRoute] Guid roleID)
        {
            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            role.ActorId = GetUserGUID();

            var result = await IoC.RoleMgmt.DeleteAsync(role);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1"), HttpGet]
        public IActionResult GetRolesV1([FromQuery] PagingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roles = IoC.RoleMgmt.Store.Get()
                .OrderBy(model.OrderBy)
                .Skip(Convert.ToInt32((model.PageNumber - 1) * model.PageSize))
                .Take(Convert.ToInt32(model.PageSize));

            var result = roles.Select(x => new RoleFactory<AppRole>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{roleID:guid}"), HttpGet]
        public async Task<IActionResult> GetRoleV1([FromRoute] Guid roleID)
        {
            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(BaseLib.Statics.MsgRoleNotExist);

            var result = new RoleFactory<AppRole>(role);

            return Ok(result.Evolve());
        }

        [Route("v1/{roleName}"), HttpGet]
        public async Task<IActionResult> GetRoleV1([FromRoute] string roleName)
        {
            var role = await IoC.RoleMgmt.FindByNameAsync(roleName.ToString());

            if (role == null)
                return NotFound(BaseLib.Statics.MsgRoleNotExist);

            var result = new RoleFactory<AppRole>(role);

            return Ok(result.Evolve());
        }

        [Route("v1/{roleID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetRoleUsersV1([FromRoute] Guid roleID)
        {
            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(BaseLib.Statics.MsgRoleNotExist);

            var users = IoC.RoleMgmt.GetUsersListAsync(role);

            var result = users.Select(x => new UserFactory<AppUser>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{roleID:guid}/remove/{userID:guid}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveRoleFromUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await IoC.RoleMgmt.FindByIdAsync(roleID.ToString());

            if (role == null)
                return NotFound(BaseLib.Statics.MsgRoleNotExist);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var result = await IoC.UserMgmt.RemoveFromRoleAsync(user, role.Name);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateRoleV1([FromBody] RoleUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var role = await IoC.RoleMgmt.FindByIdAsync(model.Id.ToString());

            if (role == null)
                return NotFound(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            var update = new RoleFactory<AppRole>(role);
            update.Update(model);

            var result = await IoC.RoleMgmt.UpdateAsync(update.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok(update.Evolve());
        }
    }
}
