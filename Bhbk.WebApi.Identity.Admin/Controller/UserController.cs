using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
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
        public async Task<IHttpActionResult> CreateUser(UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByNameAsync(model.Email);

            if (user == null)
            {
                var create = UoW.UserMgmt.Store.Mf.Create.DoIt(model);
                var result = await UoW.UserMgmt.CreateAsync(create);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(await UoW.UserMgmt.FindByNameAsync(model.Email));
            }
            else
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);
        }

        [Route("v1/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteUser(Guid userID)
        {
            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                var result = await UoW.UserMgmt.DeleteAsync(user.Id);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpGet]
        public async Task<IHttpActionResult> GetUser(Guid userID)
        {
            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(user);
        }

        [Route("v1/{username}"), HttpGet]
        public async Task<IHttpActionResult> GetUserByName(string username)
        {
            var user = await UoW.UserMgmt.FindByNameAsync(username);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(user);
        }

        [Route("v1/{userID}/providers"), HttpGet]
        public async Task<IHttpActionResult> GetUserProviders(Guid userID)
        {
            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var list = await UoW.UserMgmt.GetProvidersAsync(userID);
                IList<ProviderModel> result = new List<ProviderModel>();

                foreach(string name in list)
                {
                    var provider = UoW.ProviderMgmt.Store.Get(x => x.Name == name).Single();

                    result.Add(new ProviderModel()
                    {
                        Id = provider.Id,
                        Name = provider.Name,
                        Description = provider.Description,
                        Enabled = provider.Enabled,
                        Created = provider.Created,
                        LastUpdated = provider.LastUpdated,
                        Immutable = provider.Immutable
                    });
                }

                return Ok(result);
            }
        }

        [Route("v1/{userID}/roles"), HttpGet]
        public async Task<IHttpActionResult> GetUserRoles(Guid userID)
        {
            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var result = await UoW.UserMgmt.GetRolesAsync(userID);
                return Ok(result);
            }
        }

        [Route("v1"), HttpGet]
        public async Task<IHttpActionResult> GetUsers()
        {
            return Ok(await UoW.UserMgmt.GetListAsync());
        }

        [Route("v1/{userID}/add-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> AddPassword(Guid userID, UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.UserMgmt.AddPasswordAsync(userID, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}/remove-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> RemovePassword(Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (!await UoW.UserMgmt.HasPasswordAsync(user.Id))
                return BadRequest(BaseLib.Statics.MsgUserPasswordNotExists);

            else
            {
                IdentityResult result = await UoW.UserMgmt.RemovePasswordAsync(userID);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}/reset-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> ResetPassword(Guid userID, UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            else
            {
                string token = await UoW.UserMgmt.GeneratePasswordResetTokenAsync(userID);
                IdentityResult result = await UoW.UserMgmt.ResetPasswordAsync(userID, token, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateUser(Guid userID, UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (userID != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var user = await UoW.UserMgmt.FindByIdAsync(model.Id);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            else
            {
                user.LockoutEnabled = model.LockoutEnabled;
                user.LockoutEndDateUtc = model.LockoutEndDateUtc.HasValue ? model.LockoutEndDateUtc.Value.ToUniversalTime() : model.LockoutEndDateUtc;

                var update = UoW.UserMgmt.Store.Mf.Update.DoIt(model);
                var devolve = UoW.UserMgmt.Store.Mf.Devolve.DoIt(update);
                IdentityResult result = await UoW.UserMgmt.UpdateAsync(devolve);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(update);
            }
        }
    }
}
