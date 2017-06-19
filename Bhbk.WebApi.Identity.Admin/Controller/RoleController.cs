using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("role")]
    [Authorize(Roles = "(Built-In) Administrators")]
    public class RoleController : BaseController
    {
        public RoleController() { }

        public RoleController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/{roleID}/add/{userID}"), HttpPost]
        public async Task<IHttpActionResult> AddRoleToUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await UoW.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.UserMgmt.AddToRoleAsync(user.Id, role.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpPost]
        public async Task<IHttpActionResult> CreateRole(RoleModel.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await UoW.RoleMgmt.FindByNameAsync(model.Name);

            if (role == null)
            {
                model.Immutable = false;

                var create = UoW.Models.Create.DoIt(model);
                var result = await UoW.RoleMgmt.CreateAsync(create);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(create);
            }
            else
                return BadRequest(BaseLib.Statics.MsgRoleAlreadyExists);
        }

        [Route("v1/{roleID}"), HttpDelete]
        public async Task<IHttpActionResult> DeleteRole(Guid roleID)
        {
            var role = await UoW.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                IdentityResult result = await UoW.RoleMgmt.DeleteAsync(roleID);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetRoleList()
        {
            return Ok(await UoW.RoleMgmt.GetListAsync());
        }

        [Route("v1/{roleID}"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetRole(Guid roleID)
        {
            var role = await UoW.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else
                return Ok(role);
        }

        [Route("v1/{roleID}/users"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetRoleUsers(Guid roleID)
        {
            var role = await UoW.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else
                return Ok(await UoW.RoleMgmt.GetUsersAsync(roleID));
        }

        [Route("v1/{roleID}/remove/{userID}"), HttpDelete]
        public async Task<IHttpActionResult> RemoveRoleFromUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await UoW.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.UserMgmt.RemoveFromRoleAsync(userID, role.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{roleID}"), HttpPut]
        public async Task<IHttpActionResult> UpdateRole(Guid roleID, RoleModel.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (roleID != model.Id)
                return BadRequest(BaseLib.Statics.MsgRoleInvalid);

            var role = await UoW.RoleMgmt.FindByIdAsync(roleID);

            if (role == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                role.Name = model.Name;
                role.Description = model.Description;
                role.AudienceId = model.AudienceId;
                role.Immutable = false;

                IdentityResult result = await UoW.RoleMgmt.UpdateAsync(model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(role);
            }
        }
    }
}
