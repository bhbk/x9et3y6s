using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    public class ProviderManager : IGenericManager<ProviderModel, Guid>
    {
        public ProviderStore Store;

        public ProviderManager(ProviderStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
        }

        public async Task<ProviderModel> CreateAsync(ProviderModel model)
        {
            var provider = Store.Mf.Devolve.DoIt(model);

            if (!Store.Exists(provider.Name))
            {
                var result = Store.Create(provider);
                return Store.Mf.Evolve.DoIt(result);
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<bool> DeleteAsync(Guid clientId)
        {
            if (Store.Exists(clientId))
                return Store.Delete(clientId);
            else
                throw new ArgumentNullException();
        }

        public async Task<ProviderModel> FindByIdAsync(Guid providerId)
        {
            var provider = Store.FindById(providerId);

            if (provider == null)
                return null;

            return Store.Mf.Evolve.DoIt(provider);
        }

        public async Task<ProviderModel> FindByNameAsync(string providerName)
        {
            var provider = Store.FindByName(providerName);

            if (provider == null)
                return null;

            return Store.Mf.Evolve.DoIt(provider);
        }

        public async Task<IList<ProviderModel>> GetListAsync()
        {
            IList<ProviderModel> result = new List<ProviderModel>();
            var providers = Store.GetAll();

            foreach (AppProvider provider in providers)
                result.Add(Store.Mf.Evolve.DoIt(provider));

            return result;
        }

        public async Task<IList<UserModel>> GetUsersListAsync(Guid providerId)
        {
            IList<UserModel> result = new List<UserModel>();
            var list = Store.GetUsers(providerId);

            foreach (AppUser entry in list)
                result.Add(Store.Mf.Evolve.DoIt(entry));

            return result;
        }

        public async Task<bool> IsInProviderAsync(Guid providerId, string user)
        {
            if (Store.Exists(providerId))
                return Store.IsRoleInProvider(providerId, user);
            else
                throw new ArgumentNullException();
        }

        public async Task<ProviderModel> UpdateAsync(ProviderModel model)
        {
            var provider = Store.Mf.Devolve.DoIt(model);

            if (Store.Exists(provider.Id))
            {
                var result = Store.Update(provider);
                return Store.Mf.Evolve.DoIt(result);
            }
            else
                throw new ArgumentNullException();
        }
    }
}
