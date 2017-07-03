﻿using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    public class AudienceManager : IGenericManager<AudienceModel, Guid>
    {
        public AudienceStore Store;

        public AudienceManager(AudienceStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
        }

        public async Task<AudienceModel> CreateAsync(AudienceModel model)
        {
            var audience = Store.Mf.Devolve.DoIt(model);

            if (!Store.Exists(audience.Name))
            {
                var result = Store.Create(audience);
                return Store.Mf.Evolve.DoIt(result);
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<bool> DeleteAsync(Guid audienceId)
        {
            if (Store.Exists(audienceId))
                return Store.Delete(audienceId);
            else
                throw new ArgumentNullException();
        }

        public async Task<AudienceModel> FindByIdAsync(Guid audienceId)
        {
            var audience = Store.FindById(audienceId);

            if (audience == null)
                return null;

            return Store.Mf.Evolve.DoIt(audience);
        }

        public async Task<AudienceModel> FindByNameAsync(string audienceName)
        {
            var audience = Store.FindByName(audienceName);

            if (audience == null)
                return null;

            return Store.Mf.Evolve.DoIt(audience);
        }

        public async Task<IList<AudienceModel>> GetListAsync()
        {
            IList<AudienceModel> result = new List<AudienceModel>();
            var audiences = Store.GetAll();

            if (audiences == null)
                return null;

            else
            {
                foreach (AppAudience audience in audiences)
                    result.Add(Store.Mf.Evolve.DoIt(audience));

                return result;
            }
        }

        public async Task<IList<RoleModel>> GetRoleListAsync(Guid audienceId)
        {
            IList<RoleModel> result = new List<RoleModel>();
            var roles = Store.GetRoles(audienceId);

            if (roles == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppRole role in roles)
                    result.Add(Store.Mf.Evolve.DoIt(role));

                return result;
            }
        }

        public async Task<AudienceModel> UpdateAsync(AudienceModel model)
        {
            var audience = Store.Mf.Devolve.DoIt(model);

            if (Store.Exists(model.Id))
            {
                var result = Store.Update(audience);
                return Store.Mf.Evolve.DoIt(result);
            }
            else
                throw new ArgumentNullException();
        }
    }
}
