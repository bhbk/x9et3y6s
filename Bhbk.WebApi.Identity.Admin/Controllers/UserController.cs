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
    [Route("user")]
    public class UserController : BaseController
    {
        public UserController() { }

        public UserController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var user = await IoC.UserMgmt.FindByNameAsync(model.Email);

            if (user != null)
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);

            var create = new UserFactory<UserCreate>(model);

            var result = await IoC.UserMgmt.CreateAsync(create.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            var verify = await IoC.UserMgmt.FindByIdAsync(create.Id.ToString());

            return Ok((new UserFactory<AppUser>(verify)).Evolve());
        }

        [Route("v1/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteUser(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                user.ActorId = GetUserGUID();

                var result = await IoC.UserMgmt.DeleteAsync(user);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/{userID}"), HttpGet]
        public async Task<IActionResult> GetUser(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.Evolve());
        }

        [Route("v1/{email}"), HttpGet]
        public async Task<IActionResult> GetUser(string email)
        {
            var user = await IoC.UserMgmt.FindByEmailAsync(email);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.Evolve());
        }

        [Route("v1/{userID}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLogins(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var logins = await IoC.UserMgmt.GetLoginsAsync(user);

            var result = IoC.LoginMgmt.Store.Get(x => logins.Contains(x.Id.ToString()))
                .Select(x => new LoginFactory<AppLogin>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1/{userID}/audiences"), HttpGet]
        public async Task<IActionResult> GetUserAudiences(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var audiences = await IoC.UserMgmt.GetAudiencesAsync(user);

            var result = IoC.AudienceMgmt.Store.Get(x => audiences.Contains(x.Id.ToString()))
                .Select(x => new AudienceFactory<AppAudience>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1/{userID}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRoles(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var roles = await IoC.UserMgmt.GetRolesReturnIdAsync(user);

            var result = IoC.RoleMgmt.Store.Get(x => roles.Contains(x.Id.ToString()))
                .Select(x => new RoleFactory<AppRole>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] CustomPagingModel filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var users = IoC.UserMgmt.Store.Get().AsQueryable()
                .OrderBy(filter.OrderBy)
                .Skip(Convert.ToInt32((filter.PageNumber - 1) * filter.PageSize))
                .Take(Convert.ToInt32(filter.PageSize));

            var result = users.Select(x => new UserFactory<AppUser>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1/{userID}/add-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddPassword(Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                user.ActorId = GetUserGUID();

                var result = await IoC.UserMgmt.AddPasswordAsync(user, model.NewPassword);

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

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (!await IoC.UserMgmt.HasPasswordAsync(user))
                return BadRequest(BaseLib.Statics.MsgUserInvalidPassword);

            else
            {
                user.ActorId = GetUserGUID();

                var result = await IoC.UserMgmt.RemovePasswordAsync(user);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/{userID}/reset-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> ResetPassword(Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            user.ActorId = GetUserGUID();

            if (user.HumanBeing)
            {
                var token = await IoC.UserMgmt.GeneratePasswordResetTokenAsync(user);
                var reset = await IoC.UserMgmt.ResetPasswordAsync(user, token, model.NewPassword);

                if (!reset.Succeeded)
                    return GetErrorResult(reset);

                return NoContent();
            }
            else
            {
                var remove = await IoC.UserMgmt.RemovePasswordAsync(user);

                if (!remove.Succeeded)
                    return GetErrorResult(remove);

                var add = await IoC.UserMgmt.AddPasswordAsync(user, model.NewPassword);

                if (!add.Succeeded)
                    return GetErrorResult(add);

                return NoContent();
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var user = await IoC.UserMgmt.FindByIdAsync(model.Id.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                var update = new UserFactory<AppUser>(user);
                update.Update(model);

                var result = await IoC.UserMgmt.UpdateAsync(update.Devolve());

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update.Evolve());
            }
        }
    }
}
