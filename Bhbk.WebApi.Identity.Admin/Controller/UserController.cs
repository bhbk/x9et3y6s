﻿using Bhbk.Lib.Identity.Infrastructure;
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
    [Authorize(Roles = "(Built-In) Administrators")]
    public class UserController : BaseController
    {
        public UserController() { }

        public UserController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpPost]
        public async Task<IHttpActionResult> CreateUser(UserModel.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByNameAsync(model.Email);

            if (user == null)
            {
                var result = await UoW.UserMgmt.CreateAsync(model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(await UoW.UserMgmt.FindByNameAsync(model.Email));
            }
            else
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);
        }

        [Route("v1/{userID}"), HttpDelete]
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
        [Authorize]
        public async Task<IHttpActionResult> GetUser(Guid userID)
        {
            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(user);
        }

        [Route("v1/{username}"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetUserByName(string username)
        {
            var user = await UoW.UserMgmt.FindByNameAsync(username);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
                return Ok(user);
        }

        [Route("v1/{userID}/providers"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetUserProviders(Guid userID)
        {
            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var list = await UoW.UserMgmt.GetProvidersAsync(userID);
                IList<ProviderModel.Model> result = new List<ProviderModel.Model>();

                foreach(string name in list)
                {
                    var provider = UoW.ProviderMgmt.LocalStore.Get(x => x.Name == name).Single();

                    result.Add(new ProviderModel.Model()
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
        [Authorize]
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
        [Authorize]
        public async Task<IHttpActionResult> GetUserList()
        {
            return Ok(await UoW.UserMgmt.GetListAsync());
        }

        [Route("v1/{userID}/add-password"), HttpPut]
        public async Task<IHttpActionResult> AddPassword(Guid userID, UserModel.AddPassword model)
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
        public async Task<IHttpActionResult> ResetPassword(Guid userID, UserModel.AddPassword model)
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
        public async Task<IHttpActionResult> UpdateUser(Guid userID, UserModel.Update model)
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
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.LockoutEnabled = model.LockoutEnabled;
                user.LockoutEndDateUtc = model.LockoutEndDateUtc.HasValue ? model.LockoutEndDateUtc.Value.ToUniversalTime() : model.LockoutEndDateUtc;
                user.Immutable = false;

                IdentityResult result = await UoW.UserMgmt.UpdateAsync(UoW.Models.Devolve.DoIt(user));

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(user);
            }
        }
    }
}
