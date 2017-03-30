using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("role")]
    public class RoleController : BaseController
    {
        public RoleController() { }

        public RoleController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpGet]
        public IHttpActionResult GetRoles()
        {
            return Ok(UoW.CustomRoleManager.Roles.ToList().Select(x => ModelFactory.Create(x)));
        }

        [Route("v1/{roleID}"), HttpGet]
        public async Task<IHttpActionResult> GetRole(Guid roleID)
        {
            var foundRole = await UoW.CustomRoleManager.FindByIdAsync(roleID);

            if (foundRole == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else
                return Ok(ModelFactory.Create(foundRole));
        }

        [Route("v1/{roleID}/users"), HttpGet]
        public async Task<IHttpActionResult> GetUsersInRole(Guid roleID)
        {
            var foundRole = await UoW.RoleRepository.FindAsync(roleID);

            if (foundRole == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else
                return Ok(foundRole.Roles.Where(x => x.RoleId == foundRole.Id).ToList().Select(y => ModelFactory.Create(y.Users)));
        }

        [Route("v1"), HttpPost]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateRole(RoleModel.Binding.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundRole = UoW.RoleRepository.Get(x => x.Name == model.Name).SingleOrDefault();

            if (foundRole == null)
            {
                var role = new AppRole
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description,
                    Immutable = false,
                    AudienceId = model.AudienceId
                };

                IdentityResult result = await UoW.CustomRoleManager.CreateAsync(role);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(ModelFactory.Create(role));
            }
            else
                return BadRequest(BaseLib.Statics.MsgRoleAlreadyExists);
        }

        [Route("v1/{roleID}"), HttpPut]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateRole(Guid roleID, RoleModel.Binding.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (roleID != model.Id)
                return BadRequest(BaseLib.Statics.MsgRoleInvalid);

            var foundRole = await UoW.CustomRoleManager.FindByIdAsync(roleID);

            if (foundRole == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (foundRole.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                foundRole.Name = model.Name;
                foundRole.Description = model.Description;
                foundRole.AudienceId = model.AudienceId;

                IdentityResult result = await UoW.CustomRoleManager.UpdateAsync(foundRole);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(ModelFactory.Create(foundRole));
            }
        }

        [Route("v1/{roleID}"), HttpDelete]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteRole(Guid roleID)
        {
            var foundRole = await UoW.CustomRoleManager.FindByIdAsync(roleID);

            if (foundRole == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (foundRole.Immutable)
                return BadRequest(BaseLib.Statics.MsgRoleImmutable);

            else
            {
                IdentityResult result = await UoW.CustomRoleManager.DeleteAsync(foundRole);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{roleID}/add/{userID}"), HttpPost]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> AddRoleToUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundRole = await UoW.CustomRoleManager.FindByIdAsync(roleID);
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundRole == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.AddToRoleAsync(foundUser.Id, foundRole.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{roleID}/remove/{userID}"), HttpDelete]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> RemoveRoleFromUser(Guid roleID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundRole = await UoW.CustomRoleManager.FindByIdAsync(roleID);
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundRole == null)
                return BadRequest(BaseLib.Statics.MsgRoleNotExist);

            else if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.RemoveFromRoleAsync(userID, UoW.RoleRepository.Find(roleID).Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }
    }
}
