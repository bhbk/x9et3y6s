using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Managers
{
    public class AudienceManager : IGenericManager<AppAudience, Guid>
    {
        public readonly AudienceStore Store;

        public AudienceManager(AudienceStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
        }

        public async Task<AppAudience> CreateAsync(AppAudience audience)
        {
            if (Store.Exists(audience.Name))
                throw new InvalidOperationException();

            return Store.Create(audience);
        }

        public async Task<bool> DeleteAsync(AppAudience audience)
        {
            if (!Store.Exists(audience.Id))
                throw new InvalidOperationException();

            return Store.Delete(audience);
        }

        public async Task<AppAudience> FindByIdAsync(Guid audienceId)
        {
            return Store.FindById(audienceId);
        }

        public async Task<AppAudience> FindByNameAsync(string audienceName)
        {
            return Store.FindByName(audienceName);
        }

        public async Task<IList<AppRole>> GetRoleListAsync(Guid audienceId)
        {
            IList<AppRole> result = new List<AppRole>();
            var roles = Store.GetRoles(audienceId);

            if (roles == null)
                throw new InvalidOperationException();

            foreach (AppRole role in roles)
                result.Add(role);

            return result;
        }

        public async Task<AppAudience> UpdateAsync(AppAudience audience)
        {
            if (!Store.Exists(audience.Id))
                throw new InvalidOperationException();

            return Store.Update(audience);
        }
    }
}
