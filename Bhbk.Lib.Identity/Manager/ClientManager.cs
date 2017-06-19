using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    public class ClientManager
    {
        public ClientStore LocalStore;

        public ClientManager(ClientStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            LocalStore = store;
        }

        public async Task<IdentityResult> CreateAsync(ClientModel.Model client)
        {
            if (!LocalStore.Exists(client.Id))
            {
                await LocalStore.Create(client);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> DeleteAsync(Guid clientId)
        {
            if (LocalStore.Exists(clientId))
            {
                LocalStore.Delete(clientId);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<ClientModel.Model> FindByIdAsync(Guid clientId)
        {
            return LocalStore.FindById(clientId);
        }

        public Task<ClientModel.Model> FindByNameAsync(string clientName)
        {
            return LocalStore.FindByName(clientName);
        }

        public Task<IList<ClientModel.Model>> GetListAsync()
        {
            return LocalStore.GetAll();
        }

        public Task<IList<AudienceModel.Model>> GetAudiencesAsync(Guid clientId)
        {
            if (LocalStore.Exists(clientId))
                return LocalStore.GetAudiences(clientId);
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> UpdateAsync(ClientModel.Update client)
        {
            if (LocalStore.Exists(client.Id))
            {
                LocalStore.Update(client);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }
    }
}
