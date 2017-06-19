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

        public async Task<IdentityResult> CreateAsync(AudienceModel.Model audience)
        {
            if (!LocalStore.Exists(audience.Id))
            {
                await LocalStore.Create(audience);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> DeleteAsync(Guid audienceId)
        {
            if (LocalStore.Exists(audienceId))
            {
                LocalStore.Delete(audienceId);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<AudienceModel.Model> FindByIdAsync(Guid audienceId)
        {
            return LocalStore.FindById(audienceId);
        }

        public Task<AudienceModel.Model> FindByNameAsync(string audienceName)
        {
            return LocalStore.FindByName(audienceName);
        }

        public Task<IList<AudienceModel.Model>> GetListAsync()
        {
            return LocalStore.GetAll();
        }

        public Task<IList<RoleModel.Model>> GetRoleListAsync(Guid audienceId)
        {
            if (LocalStore.Exists(audienceId))
                return LocalStore.GetRoles(audienceId);
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> UpdateAsync(AudienceModel.Update audience)
        {
            if (LocalStore.Exists(audience.Id))
            {
                LocalStore.Update(audience);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }
    }
}
