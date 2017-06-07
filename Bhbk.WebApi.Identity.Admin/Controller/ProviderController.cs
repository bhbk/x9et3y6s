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

            var foundProvider = await UoW.CustomProviderManager.FindByIdAsync(providerID);

            if (foundProvider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.AddToProviderAsync(foundUser.Id, foundProvider.Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateProvider(ProviderModel.Binding.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundProvider = UoW.ProviderRepository.Get(x => x.Name == model.Name).SingleOrDefault();

            if (foundProvider == null)
            {
                var newProvider = new AppProvider()
                {
                    Id = Guid.NewGuid(),
                    Description = model.Description,
                    Name = model.Name,
                    Enabled = model.Enabled,
                    Immutable = false
                };

                IdentityResult result = await UoW.CustomProviderManager.CreateAsync(newProvider);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(ModelFactory.Create(newProvider));
            }
            else
                return BadRequest(BaseLib.Statics.MsgProviderAlreadyExists);
        }

        [Route("v1/{providerID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteProvider(Guid providerID)
        {
            var foundProvider = await UoW.ProviderRepository.FindAsync(providerID);

            if (foundProvider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else if (foundProvider.Immutable)
                return BadRequest(BaseLib.Statics.MsgProviderImmutable);

            else
            {
                UoW.ProviderRepository.Delete(foundProvider);
                await UoW.SaveAsync();

                return Ok();
            }
        }

        [Route("v1/{providerID}"), HttpGet]
        public async Task<IHttpActionResult> GetProvider(Guid providerID)
        {
            var foundProvider = await UoW.ProviderRepository.FindAsync(providerID);

            if (foundProvider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else
                return Ok(ModelFactory.Create(foundProvider));
        }

        [Route("v1"), HttpGet]
        public IHttpActionResult GetProviders()
        {
            return Ok(UoW.ProviderRepository.Get().Select(x => ModelFactory.Create(x)));
        }

        [Route("v1/{providerID}/users"), HttpGet]
        public async Task<IHttpActionResult> GetProviderUsers(Guid providerID)
        {
            var foundProvider = await UoW.ProviderRepository.FindAsync(providerID);

            if (foundProvider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else
            {
                var foundUsers = await UoW.CustomProviderManager.GetUsersAsync(providerID);
                List<AppUser> result = new List<AppUser>();

                foreach (string user in foundUsers)
                    result.Add(UoW.UserRepository.Get(x => x.Email == user).Single());

                return Ok(result.Select(x => ModelFactory.Create(x)));
            }
        }

        [Route("v1/{roleID}/remove/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> RemoveProviderFromUser(Guid providerID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundProvider = await UoW.CustomProviderManager.FindByIdAsync(providerID);

            if (foundProvider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.RemoveFromProviderAsync(userID, UoW.ProviderRepository.Find(providerID).Name);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{providerID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateProvider(Guid providerID, ProviderModel.Binding.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (providerID != model.Id)
                return BadRequest(BaseLib.Statics.MsgProviderInvalid);

            var foundProvider = await UoW.ProviderRepository.FindAsync(providerID);

            if (foundProvider == null)
                return BadRequest(BaseLib.Statics.MsgProviderNotExist);

            else if (foundProvider.Immutable)
                return BadRequest(BaseLib.Statics.MsgProviderImmutable);

            else
            {
                foundProvider.Name = model.Name;
                foundProvider.Description = model.Description;
                foundProvider.Enabled = model.Enabled;
                foundProvider.Immutable = false;

                IdentityResult result = await UoW.CustomProviderManager.UpdateAsync(foundProvider);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(ModelFactory.Create(foundProvider));
            }
        }
    }
}