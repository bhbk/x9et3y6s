using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
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

            if (user != null)
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);

            var create = new UserFactory<AppUser>(model);
            var result = await Context.UserMgmt.CreateAsync(create.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            else
                return Ok(create.Evolve());
        }

        [Route("v1/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteUser(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                var result = await Context.UserMgmt.DeleteAsync(user);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/{userID}"), HttpGet]
        public async Task<IActionResult> GetUser(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.Evolve());
        }

        [Route("v1/{username}"), HttpGet]
        public async Task<IActionResult> GetUserByName(string username)
        {
            var user = await Context.UserMgmt.FindByNameAsync(username);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.Evolve());
        }

        [Route("v1/{userID}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLogins(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            var result = await Context.UserMgmt.GetLoginsAsync(user);

            return Ok(result);
        }

        [Route("v1/{userID}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRoles(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            var result = await Context.UserMgmt.GetRolesAsync(user);

            return Ok(result);
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            IList<UserResult> result = new List<UserResult>();
            var users = await Context.UserMgmt.GetListAsync();

            foreach (AppUser entry in users)
                result.Add(new UserFactory<AppUser>(entry).Evolve());

            return Ok(result);
        }

        [Route("v1/{userID}/add-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddPassword(Guid userID, UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                var result = await Context.UserMgmt.AddPasswordAsync(user, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/{userID}/remove-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemovePassword(Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await Context.UserMgmt.HasPasswordAsync(user))
                return BadRequest(BaseLib.Statics.MsgUserPasswordNotExists);

            else
            {
                var result = await Context.UserMgmt.RemovePasswordAsync(user);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/{userID}/reset-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> ResetPassword(Guid userID, UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                string token = await Context.UserMgmt.GeneratePasswordResetTokenAsync(user);
                var result = await Context.UserMgmt.ResetPasswordAsync(user, token, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateUser(UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(model.Id.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                var update = new UserFactory<UserUpdate>(model);
                var result = await Context.UserMgmt.UpdateAsync(update.Devolve());

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update.Evolve());
            }
        }
    }
}
