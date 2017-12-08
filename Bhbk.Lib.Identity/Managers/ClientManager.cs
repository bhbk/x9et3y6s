using Bhbk.Lib.Identity.Factory;
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
        public ClientStore Store;

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

        public async Task<bool> DeleteAsync(Guid clientId)
        {
            if (!Store.Exists(clientId))
                throw new InvalidOperationException();

            return Store.Delete(clientId);
        }

        public async Task<AppClient> FindByIdAsync(Guid clientId)
        {
            var client = Store.FindById(clientId);

            if (client == null)
                return null;

            return client;
        }

        public async Task<AppClient> FindByNameAsync(string clientName)
        {
            var client = Store.FindByName(clientName);

            if (client == null)
                return null;

            return client;
        }

        public async Task<IList<AppClient>> GetListAsync()
        {
            return Store.GetAll();
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
