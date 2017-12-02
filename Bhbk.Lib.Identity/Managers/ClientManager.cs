using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Managers
{
    public class ClientManager : IGenericManager<ClientModel, Guid>
    {
        public ClientStore Store;

        public ClientManager(ClientStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
        }

        public async Task<ClientModel> CreateAsync(ClientModel model)
        {
            var client = Store.Mf.Devolve.DoIt(model);

            if (Store.Exists(client.Name))
                throw new InvalidOperationException();

            var result = Store.Create(client);

            return Store.Mf.Evolve.DoIt(result);
        }

        public async Task<bool> DeleteAsync(Guid clientId)
        {
            if (!Store.Exists(clientId))
                throw new InvalidOperationException();

            return Store.Delete(clientId);
        }

        public async Task<ClientModel> FindByIdAsync(Guid clientId)
        {
            var client = Store.FindById(clientId);

            if (client == null)
                return null;

            return Store.Mf.Evolve.DoIt(client);
        }

        public async Task<ClientModel> FindByNameAsync(string clientName)
        {
            var client = Store.FindByName(clientName);

            if (client == null)
                return null;

            return Store.Mf.Evolve.DoIt(client);
        }

        public async Task<IList<ClientModel>> GetListAsync()
        {
            IList<ClientModel> result = new List<ClientModel>();
            var clients = Store.GetAll();

            if (clients == null)
                throw new InvalidOperationException();

            foreach (AppClient client in clients)
                result.Add(Store.Mf.Evolve.DoIt(client));

            return result;
        }

        public async Task<IList<AudienceModel>> GetAudiencesAsync(Guid clientId)
        {
            IList<AudienceModel> result = new List<AudienceModel>();
            var audiences = Store.GetAudiences(clientId);

            if (audiences == null)
                throw new InvalidOperationException();

            foreach (AppAudience audience in audiences)
                result.Add(Store.Mf.Evolve.DoIt(audience));

            return result;
        }

        public async Task<ClientModel> UpdateAsync(ClientModel model)
        {
            var client = Store.Mf.Devolve.DoIt(model);

            if (!Store.Exists(client.Id))
                throw new InvalidOperationException();

            var result = Store.Update(client);

            return Store.Mf.Evolve.DoIt(result);
        }
    }
}
