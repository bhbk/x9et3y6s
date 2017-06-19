using Bhbk.Lib.Identity.Infrastructure;
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
        public CustomRoleStore LocalStore;

        public CustomRoleManager(CustomRoleStore store)
            : base(store)
        {
            LocalStore = store;
        }

        public async Task<IdentityResult> CreateAsync(RoleModel.Create role)
        {
            if (!LocalStore.Exists(role.Name))
            {
                await LocalStore.CreateAsync(role);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> DeleteAsync(Guid roleId)
        {
            if (LocalStore.Exists(roleId))
            {
                await LocalStore.DeleteAsync(roleId);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }

        public async Task<RoleModel.Model> FindByIdAsync(Guid roleId)
        {
            return await LocalStore.FindById(roleId);
        }

        public async Task<RoleModel.Model> FindByNameAsync(string roleName)
        {
            return await LocalStore.FindByName(roleName);
        }

        public async Task<IList<RoleModel.Model>> GetListAsync()
        {
            return await LocalStore.GetAllAsync();
        }

        public async Task<IList<UserModel.Model>> GetUsersAsync(Guid roleId)
        {
            if (LocalStore.Exists(roleId))
                return await LocalStore.GetUsersAsync(roleId);
            else
                throw new ArgumentNullException();
        }

        public async Task<IdentityResult> UpdateAsync(RoleModel.Update role)
        {
            if (LocalStore.Exists(role.Id))
            {
                await LocalStore.UpdateAsync(role);
                return IdentityResult.Success;
            }
            else
                throw new ArgumentNullException();
        }
    }
}