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

        public Task<IdentityResult> CreateAsync(RoleModel.Model role)
        {
            if (!LocalStore.Exists(role.Id))
            {
                LocalStore.CreateAsync(role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> DeleteAsync(Guid roleId)
        {
            if (LocalStore.Exists(roleId))
            {
                LocalStore.DeleteAsync(roleId);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<RoleModel.Model> FindByIdAsync(Guid roleId)
        {
            return LocalStore.FindByIdAsync(roleId);
        }

        public Task<RoleModel.Model> FindByNameAsync(string roleName)
        {
            return LocalStore.FindByNameAsync(roleName);
        }

        public Task<IList<RoleModel.Model>> GetListAsync()
        {
            return LocalStore.GetAllAsync();
        }

        public Task<IList<UserModel.Model>> GetUsersAsync(Guid roleId)
        {
            if (LocalStore.Exists(roleId))
                return LocalStore.GetUsersAsync(roleId);
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> UpdateAsync(RoleModel.Update role)
        {
            if (LocalStore.Exists(role.Id))
            {
                LocalStore.UpdateAsync(role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }
    }
}