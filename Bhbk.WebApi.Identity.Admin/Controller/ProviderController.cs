using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("provider")]
    public class ProviderController : BaseController
    {
        public ProviderController() { }

        public ProviderController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/{providerID}/add/{userID}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
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
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateProvider(ProviderCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var provider = await UoW.ProviderMgmt.FindByNameAsync(model.Name);

            if (provider == null)
            {
                var create = UoW.UserMgmt.Store.Mf.Create.DoIt(model);
                var result = await UoW.ProviderMgmt.CreateAsync(create);

                return Ok(result);
            }
            else
                return BadRequest(BaseLib.Statics.MsgProviderAlreadyExists);
        }

        [Route("v1/{providerID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteProvider(Guid providerID)
        {
            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else if (provider.Immutable)
                return BadRequest(BaseLib.Statics.MsgProviderImmutable);

            if (!await UoW.ProviderMgmt.DeleteAsync(providerID))
                return InternalServerError();

            else
                return Ok();
        }

        [Route("v1/{providerID}"), HttpGet]
        public async Task<IHttpActionResult> GetProvider(Guid providerID)
        {
            var provider = await UoW.ProviderMgmt.FindByIdAsync(providerID);

            if (provider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else
                return Ok(provider);
        }

        [Route("v1"), HttpGet]
        public async Task<IHttpActionResult> GetProviders()
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
        [Authorize(Roles = "(Built-In) Administrators")]
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
                IdentityResult result = await UoW.UserMgmt.RemoveFromProviderAsync(userID, UoW.ProviderMgmt.Store.FindById(providerID).Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{providerID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateProvider(Guid providerID, ProviderUpdate model)
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
                var update = UoW.ProviderMgmt.Store.Mf.Update.DoIt(model);
                var result = await UoW.ProviderMgmt.UpdateAsync(update);

                return Ok(result);
            }
        }
    }
}