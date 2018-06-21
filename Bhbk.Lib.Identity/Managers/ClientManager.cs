using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Managers
{
    public class ClientManager : IGenericManager<AppClient, Guid>
    {
        public readonly ClientStore Store;

        public ClientManager(ClientStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
        }

        public async Task<AppClient> CreateAsync(AppClient client)
        {
            if (Store.Exists(client.Name))
                throw new InvalidOperationException();

            return Store.Create(client);
        }

        public async Task<bool> DeleteAsync(AppClient client)
        {
            if (!Store.Exists(client.Id))
                throw new InvalidOperationException();

            return Store.Delete(client);
        }

        public async Task<AppClient> FindByIdAsync(Guid clientId)
        {
            return Store.FindById(clientId);
        }

        public async Task<AppClient> FindByNameAsync(string clientName)
        {
            return Store.FindByName(clientName);
        }

        public async Task<IList<AppClient>> GetListAsync()
        {
            return Store.Get();
        }

        public async Task<IList<AppAudience>> GetAudiencesAsync(Guid clientId)
        {
            if (!Store.Exists(clientId))
                throw new InvalidOperationException();

            return Store.GetAudiences(clientId);
        }

        public async Task<AppClient> UpdateAsync(AppClient client)
        {
            if (!Store.Exists(client.Id))
                throw new InvalidOperationException();

            return Store.Update(client);
        }
    }
}
