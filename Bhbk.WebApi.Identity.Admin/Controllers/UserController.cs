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
    [Route("user")]
    public class UserController : BaseController
    {
        public UserController() { }

        public UserController(IIdentityContext context)
            : base(context) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateUser(UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByNameAsync(model.Email);

            if (user == null)
            {
                var create = Context.UserMgmt.Store.Mf.Create.DoIt(model);
                var result = await Context.UserMgmt.CreateAsync(create);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(await Context.UserMgmt.FindByNameAsync(model.Email));
            }
            else
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);
        }

        [Route("v1/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteUser(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                var result = await Context.UserMgmt.DeleteAsync(user.Id);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpGet]
        public async Task<IActionResult> GetUser(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(user);
        }

        [Route("v1/{username}"), HttpGet]
        public async Task<IActionResult> GetUserByName(string username)
        {
            var user = await Context.UserMgmt.FindByNameAsync(username);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(user);
        }

        [Route("v1/{userID}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLogins(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var result = await Context.UserMgmt.GetLoginsAsync(userID);
                return Ok(result);
            }
        }

        [Route("v1/{userID}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRoles(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var result = await Context.UserMgmt.GetRolesAsync(userID);
                return Ok(result);
            }
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await Context.UserMgmt.GetListAsync());
        }

        [Route("v1/{userID}/add-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddPassword(Guid userID, UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await Context.UserMgmt.AddPasswordAsync(userID, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}/remove-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemovePassword(Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await Context.UserMgmt.HasPasswordAsync(user.Id))
                return BadRequest(BaseLib.Statics.MsgUserPasswordNotExists);

            else
            {
                IdentityResult result = await Context.UserMgmt.RemovePasswordAsync(userID);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}/reset-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> ResetPassword(Guid userID, UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                string token = await Context.UserMgmt.GeneratePasswordResetTokenAsync(userID);
                IdentityResult result = await Context.UserMgmt.ResetPasswordAsync(userID, token, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateUser(Guid userID, UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (userID != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var user = await Context.UserMgmt.FindByIdAsync(model.Id);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                user.LockoutEnabled = model.LockoutEnabled;
                user.LockoutEnd = model.LockoutEnd.HasValue ? model.LockoutEnd.Value.ToUniversalTime() : model.LockoutEnd;

                var update = Context.UserMgmt.Store.Mf.Update.DoIt(model);
                var devolve = Context.UserMgmt.Store.Mf.Devolve.DoIt(update);
                IdentityResult result = await Context.UserMgmt.UpdateAsync(devolve);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update);
            }
        }
    }
}
