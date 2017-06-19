using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    public class ProviderManager
    {
        public ProviderStore LocalStore;

        public ProviderManager(ProviderStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            LocalStore = store;
        }

        public async Task<IdentityResult> CreateAsync(ProviderModel.Model provider)
        {
            if (!LocalStore.Exists(provider.Id))
            {
                await LocalStore.Create(provider);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> DeleteAsync(Guid providerId)
        {
            if (LocalStore.Exists(providerId))
            {
                LocalStore.Delete(providerId);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<ProviderModel.Model> FindByIdAsync(Guid providerId)
        {
            return LocalStore.FindById(providerId);
        }

        public Task<ProviderModel.Model> FindByNameAsync(string providerName)
        {
            return LocalStore.FindByName(providerName);
        }

        public Task<IList<ProviderModel.Model>> GetListAsync()
        {
            return LocalStore.GetAll();
        }

        public Task<IList<UserModel.Model>> GetUsersListAsync(Guid providerId)
        {
            if (LocalStore.Exists(providerId))
                return LocalStore.GetUsers(providerId);
            else
                throw new ArgumentNullException();
        }

        public Task<bool> IsInProviderAsync(Guid providerId, string user)
        {
            if (LocalStore.Exists(providerId))
                return LocalStore.IsInProvider(providerId, user);
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> UpdateAsync(ProviderModel.Update provider)
        {
            if (LocalStore.Exists(provider.Id))
            {
                LocalStore.Update(provider);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }
    }
}
