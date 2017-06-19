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

        public async Task<IdentityResult> CreateAsync(ClientModel.Create client)
        {
            if (!LocalStore.Exists(client.Name))
            {
                await LocalStore.Create(client);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> DeleteAsync(Guid clientId)
        {
            if (LocalStore.Exists(clientId))
            {
                await LocalStore.Delete(clientId);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<ClientModel.Model> FindByIdAsync(Guid clientId)
        {
            return await LocalStore.FindById(clientId);
        }

        public async Task<ClientModel.Model> FindByNameAsync(string clientName)
        {
            return await LocalStore.FindByName(clientName);
        }

        public async Task<IList<ClientModel.Model>> GetListAsync()
        {
            return await LocalStore.GetAll();
        }

        public async Task<IList<AudienceModel.Model>> GetAudiencesAsync(Guid clientId)
        {
            if (LocalStore.Exists(clientId))
                return await LocalStore.GetAudiences(clientId);
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> UpdateAsync(ClientModel.Update client)
        {
            if (LocalStore.Exists(client.Id))
            {
                await LocalStore.Update(client);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }
    }
}
