using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("user")]
    public class UserController : BaseController
    {
        public UserController() { }

        public UserController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateUser(UserModel.Binding.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundUser = await UoW.CustomUserManager.FindByNameAsync(model.Email);

            if (foundUser == null)
            {
                var newUser = new AppUser()
                {
                    Id = Guid.NewGuid(),
                    Email = model.Email,
                    EmailConfirmed = model.EmailConfirmed,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    LockoutEnabled = model.LockoutEnabled,
                    Immutable = false
                };

                IdentityResult result = await UoW.CustomUserManager.CreateAsync(newUser);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(ModelFactory.Create(newUser));
            }
            else
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);
        }

        [Route("v1/{userID}/delete-token/{audienceID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteToken(Guid userID, Guid audienceID)
        {
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            var foundAudience = UoW.AudienceRepository.Get(x => x.Id == audienceID).SingleOrDefault();

            if (foundAudience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            var foundToken = foundUser.Tokens.Where(x => x.AudienceId == audienceID).SingleOrDefault();

            if (foundToken == null)
                return BadRequest(BaseLib.Statics.MsgUserTokenInvalid);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.RemoveRefreshTokenAsync(foundToken.Id.ToString());

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteUser(Guid userID)
        {
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (foundUser.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.DeleteAsync(foundUser);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpGet]
        public async Task<IHttpActionResult> GetUser(Guid userID)
        {
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(ModelFactory.Create(foundUser));
        }

        [Route("v1/{username}"), HttpGet]
        public async Task<IHttpActionResult> GetUserByName(string username)
        {
            var foundUser = await UoW.CustomUserManager.FindByNameAsync(username);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(ModelFactory.Create(foundUser));
        }

        [Route("v1/{userID}/providers"), HttpGet]
        public async Task<IHttpActionResult> GetUserProviders(Guid userID)
        {
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var foundProviders = await UoW.CustomUserManager.GetProvidersAsync(userID);
                List<AppProvider> result = new List<AppProvider>();

                foreach (string provider in foundProviders)
                    result.Add(UoW.ProviderRepository.Get(x => x.Name == provider).Single());

                return Ok(result.Select(x => ModelFactory.Create(x)));
            }
        }

        [Route("v1/{userID}/roles"), HttpGet]
        public async Task<IHttpActionResult> GetUserRoles(Guid userID)
        {
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var result = await UoW.CustomUserManager.GetRolesAsync(foundUser.Id);
                return Ok(result);
            }
        }

        [Route("v1"), HttpGet]
        public IHttpActionResult GetUsers()
        {
            return Ok(UoW.CustomUserManager.Users.ToList().Select(u => ModelFactory.Create(u)));
        }

        [Route("v1/{userID}/set-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> SetPassword(Guid userID, UserModel.Binding.SetPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidNewPasswordConfirm);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.SetPasswordAsync(foundUser.Id, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateUser(Guid userID, UserModel.Binding.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (userID != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(model.Id);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (foundUser.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                foundUser.UserName = model.Email;
                foundUser.Email = model.Email;
                foundUser.FirstName = model.FirstName;
                foundUser.LastName = model.LastName;
                foundUser.LockoutEnabled = model.LockoutEnabled;
                foundUser.LockoutEndDateUtc = model.LockoutEndDateUtc.HasValue ? model.LockoutEndDateUtc.Value.ToUniversalTime() : model.LockoutEndDateUtc;
                foundUser.Immutable = false;

                IdentityResult result = await UoW.CustomUserManager.UpdateAsync(foundUser);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(ModelFactory.Create(foundUser));
            }
        }
    }
}
