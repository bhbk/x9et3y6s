using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("provider")]
    [Authorize(Roles = "(Built-In) Administrators")]
    public class ProviderController : BaseController
    {
        public ProviderController() { }

        public ProviderController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/{providerID}/add/{userID}"), HttpPost]
        public async Task<IHttpActionResult> AddProviderToUser(Guid providerID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var result = await UoW.UserMgmt.AddToProviderAsync(user.Id, provider.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpPost]
        public async Task<IHttpActionResult> CreateProvider(ProviderModel.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var provider = await UoW.ProviderMgmt.FindByNameAsync(model.Name);

            if (provider == null)
            {
                model.Immutable = false;

                var result = await UoW.ProviderMgmt.CreateAsync(model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(await UoW.ProviderMgmt.FindByNameAsync(model.Name));
            }
            else
                return BadRequest(BaseLib.Statics.MsgProviderAlreadyExists);
        }

        [Route("v1/{providerID}"), HttpDelete]
        public async Task<IHttpActionResult> DeleteProvider(Guid providerID)
        {
            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else if (provider.Immutable)
                return BadRequest(BaseLib.Statics.MsgProviderImmutable);

            else
            {
                IdentityResult result = await UoW.ProviderMgmt.DeleteAsync(providerID);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{providerID}"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetProvider(Guid providerID)
        {
            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else
                return Ok(provider);
        }

        [Route("v1"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetProviderList()
        {
            return Ok(await UoW.ProviderMgmt.GetListAsync());
        }

        [Route("v1/{providerID}/users"), HttpGet]
        public async Task<IHttpActionResult> GetProviderUsers(Guid providerID)
        {
            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else
                return Ok(await UoW.ProviderMgmt.GetUsersListAsync(providerID));
        }

        [Route("v1/{roleID}/remove/{userID}"), HttpDelete]
        public async Task<IHttpActionResult> RemoveProviderFromUser(Guid providerID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            var user = await UoW.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.UserMgmt.RemoveFromProviderAsync(userID, UoW.ProviderMgmt.LocalStore.Find(providerID).Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{providerID}"), HttpPut]
        public async Task<IHttpActionResult> UpdateProvider(Guid providerID, ProviderModel.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (providerID != model.Id)
                return BadRequest(BaseLib.Statics.MsgProviderInvalid);

            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else if (provider.Immutable)
                return BadRequest(BaseLib.Statics.MsgProviderImmutable);

            else
            {
                provider.Name = model.Name;
                provider.Description = model.Description;
                provider.Enabled = model.Enabled;
                provider.Immutable = false;

                IdentityResult result = await UoW.ProviderMgmt.UpdateAsync(model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(provider);
            }
        }
    }
}