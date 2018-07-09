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

        public async Task<AppAudienceUri> AddUriAsync(AppAudienceUri audienceUri)
        {
            if (!Store.Exists(audienceUri.AudienceId))
                throw new InvalidOperationException();

            return await Task.FromResult(Store.AddUri(audienceUri));
        }

        public async Task<AppAudience> CreateAsync(AppAudience audience)
        {
            if (Store.Exists(audience.Name))
                throw new InvalidOperationException();

            return await Task.FromResult(Store.Create(audience));
        }

        public async Task<bool> DeleteAsync(AppAudience audience)
        {
            if (!Store.Exists(audience.Id))
                throw new InvalidOperationException();

            return await Task.FromResult(Store.Delete(audience));
        }

        public async Task<AppAudience> FindByIdAsync(Guid audienceId)
        {
            return await Task.FromResult(Store.FindById(audienceId));
        }

        public async Task<AppAudience> FindByNameAsync(string audienceName)
        {
            return await Task.FromResult(Store.FindByName(audienceName));
        }

        public async Task<IList<AppRole>> GetRoleListAsync(Guid audienceId)
        {
            IList<AppRole> result = new List<AppRole>();
            var roles = Store.GetRoles(audienceId);

            if (roles == null)
                throw new InvalidOperationException();

            foreach (AppRole role in roles)
                result.Add(role);

            return await Task.FromResult(result);
        }

        public async Task<AppAudience> UpdateAsync(AppAudience audience)
        {
            if (!Store.Exists(audience.Id))
                throw new InvalidOperationException();

            return await Task.FromResult(Store.Update(audience));
        }
    }
}
