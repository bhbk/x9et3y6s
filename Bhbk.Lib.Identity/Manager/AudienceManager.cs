using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    public class AudienceManager
    {
        public AudienceStore LocalStore;

        public AudienceManager(AudienceStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            LocalStore = store;
        }
        
        public async Task<IdentityResult> CreateAsync(AudienceModel.Create audience)
        {
            if (!LocalStore.Exists(audience.Name))
            {
                await LocalStore.Create(audience);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> DeleteAsync(Guid audienceId)
        {
            if (LocalStore.Exists(audienceId))
            {
                await LocalStore.Delete(audienceId);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<AudienceModel.Model> FindByIdAsync(Guid audienceId)
        {
            return await LocalStore.FindById(audienceId);
        }

        public async Task<AudienceModel.Model> FindByNameAsync(string audienceName)
        {
            return await LocalStore.FindByName(audienceName);
        }

        public async Task<IList<AudienceModel.Model>> GetListAsync()
        {
            return await LocalStore.GetAll();
        }

        public async Task<IList<RoleModel.Model>> GetRoleListAsync(Guid audienceId)
        {
            if (LocalStore.Exists(audienceId))
                return await LocalStore.GetRoles(audienceId);
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> UpdateAsync(AudienceModel.Update audience)
        {
            if (LocalStore.Exists(audience.Id))
            {
                await LocalStore.Update(audience);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }
    }
}
