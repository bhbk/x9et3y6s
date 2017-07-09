using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    public class CustomRoleManager : RoleManager<AppRole, Guid>
    {
        public CustomRoleStore Store;

        public CustomRoleManager(CustomRoleStore store)
            : base(store)
        {
            Store = store;
        }

        public async Task<IdentityResult> CreateAsync(RoleModel role)
        {
            var model = Store.Mf.Devolve.DoIt(role);

            if (Store.Exists(model.Name))
                throw new InvalidOperationException();

            await Store.CreateAsync(model);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Guid roleId)
        {
            var model = Store.FindById(roleId);

            if (model == null)
                throw new ArgumentNullException();

            await Store.DeleteAsync(model);
        
            return IdentityResult.Success;
        }

        public async Task<RoleModel> FindByIdAsync(Guid roleId)
        {
            var model = Store.FindById(roleId);

            if (model == null)
                return null;

            return Store.Mf.Evolve.DoIt(model);
        }

        public async Task<RoleModel> FindByNameAsync(string roleName)
        {
            var model = Store.FindByName(roleName);

            if (model == null)
                return null;

            return Store.Mf.Evolve.DoIt(model);
        }

        public async Task<IList<RoleModel>> GetListAsync()
        {
            IList<RoleModel> result = new List<RoleModel>();
            var roles = Store.GetAll();

            foreach (AppRole role in roles)
                result.Add(Store.Mf.Evolve.DoIt(role));

            return result;
        }

        public async Task<IList<UserModel>> GetUsersListAsync(Guid roleId)
        {
            IList<UserModel> result = new List<UserModel>();
            var list = Store.GetUsersAsync(roleId);

            if (list == null)
                throw new ArgumentNullException();

            foreach (AppUser entry in list)
                result.Add(Store.Mf.Evolve.DoIt(entry));

            return result;
        }

        public async Task<IdentityResult> UpdateAsync(RoleModel role)
        {
            var model = Store.Mf.Devolve.DoIt(role);

            if (!Store.Exists(model.Id))
                throw new InvalidOperationException();

            await Store.UpdateAsync(model);

            return IdentityResult.Success;
        }
    }
}