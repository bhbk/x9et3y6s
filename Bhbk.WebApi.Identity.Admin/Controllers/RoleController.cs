using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("role")]
    public class RoleController : BaseController
    {
        public RoleController() { }

        public RoleController(IIdentityContext context)
            : base(context) { }

        [Route("v1/{roleID}/add/{userID}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddRoleToUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await Context.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await Context.UserMgmt.AddToRoleAsync(user.Id, role.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateRole(RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await Context.RoleMgmt.FindByNameAsync(model.Name);

            if (role == null)
            {
                var create = Context.RoleMgmt.Store.Mf.Create.DoIt(model);
                IdentityResult result = await Context.RoleMgmt.CreateAsync(create);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(await Context.RoleMgmt.FindByNameAsync(model.Name));
            }
            else
                return BadRequest(BaseLib.Statics.MsgRoleAlreadyExists);
        }

        [Route("v1/{roleID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteRole(Guid roleID)
        {
            var role = await Context.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                IdentityResult result = await Context.RoleMgmt.DeleteAsync(roleID);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await Context.RoleMgmt.GetListAsync());
        }

        [Route("v1/{roleID}"), HttpGet]
        public async Task<IActionResult> GetRole(Guid roleID)
        {
            var role = await Context.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else
                return Ok(role);
        }

        [Route("v1/{roleID}/users"), HttpGet]
        public async Task<IActionResult> GetRoleUsers(Guid roleID)
        {
            var role = await Context.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else
                return Ok(await Context.RoleMgmt.GetUsersListAsync(roleID));
        }

        [Route("v1/{roleID}/remove/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveRoleFromUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await Context.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await Context.UserMgmt.RemoveFromRoleAsync(userID, role.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{roleID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateRole(Guid roleID, RoleUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (roleID != model.Id)
                return BadRequest(BaseLib.Statics.MsgRoleInvalid);

            var role = await Context.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                var update = Context.RoleMgmt.Store.Mf.Update.DoIt(model);
                IdentityResult result = await Context.RoleMgmt.UpdateAsync(update);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update);
            }
        }
    }
}
