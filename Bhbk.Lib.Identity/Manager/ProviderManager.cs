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

        public async Task<IdentityResult> CreateAsync(ProviderModel.Create provider)
        {
            if (!LocalStore.Exists(provider.Name))
            {
                await LocalStore.Create(provider);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> DeleteAsync(Guid providerId)
        {
            if (LocalStore.Exists(providerId))
            {
                await LocalStore.Delete(providerId);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<ProviderModel.Model> FindByIdAsync(Guid providerId)
        {
            return await LocalStore.FindById(providerId);
        }

        public async Task<ProviderModel.Model> FindByNameAsync(string providerName)
        {
            return await LocalStore.FindByName(providerName);
        }

        public async Task<IList<ProviderModel.Model>> GetListAsync()
        {
            return await LocalStore.GetAll();
        }

        public async Task<IList<UserModel.Model>> GetUsersListAsync(Guid providerId)
        {
            if (LocalStore.Exists(providerId))
                return await LocalStore.GetUsers(providerId);
            else
                throw new ArgumentNullException();
        }

        public async Task<bool> IsInProviderAsync(Guid providerId, string user)
        {
            if (LocalStore.Exists(providerId))
                return await LocalStore.IsRoleInProvider(providerId, user);
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> UpdateAsync(ProviderModel.Update provider)
        {
            if (LocalStore.Exists(provider.Id))
            {
                await LocalStore.Update(provider);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }
    }
}
