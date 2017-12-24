using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1

namespace Bhbk.Lib.Identity.Managers
{
    public partial class CustomRoleManager : RoleManager<AppRole>
    {
        public CustomRoleStore Store;

        public CustomRoleManager(CustomRoleStore store, 
            IEnumerable<IRoleValidator<AppRole>> roleValidators = null, 
            ILookupNormalizer keyNormalizer = null, 
            IdentityErrorDescriber errors = null, 
            ILogger<RoleManager<AppRole>> logger = null)
            : base(store, roleValidators, keyNormalizer, errors, logger)
        {
            Store = store;
        }

        public override async Task<IdentityResult> CreateAsync(AppRole role)
        {
            if (Store.Exists(role.Name))
                throw new InvalidOperationException();

            await Store.CreateAsync(role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> DeleteAsync(AppRole role)
        {
            if (!Store.Exists(role.Id))
                throw new InvalidOperationException();

            await Store.DeleteAsync(role);

            return IdentityResult.Success;
        }

        public override async Task<AppRole> FindByIdAsync(string roleId)
        {
            Guid result;

            if (!Guid.TryParse(roleId, out result))
                throw new ArgumentException();

            return await Store.FindByIdAsync(result.ToString());
        }

        public override async Task<AppRole> FindByNameAsync(string roleName)
        {
            return await Store.FindByNameAsync(roleName);
        }

        public async Task<IList<AppRole>> GetListAsync()
        {
            return Store.Get();
        }

        public async Task<IList<AppUser>> GetUsersListAsync(AppRole role)
        {
            if (!Store.Exists(role.Id))
                throw new InvalidOperationException();

            IList<AppUser> result = new List<AppUser>();
            var list = Store.GetUsersAsync(role);

            if (list == null)
                throw new ArgumentNullException();

            foreach (AppUser entry in list)
                result.Add(new UserFactory<AppUser>(entry).Devolve());

            return result;
        }

        public override async Task<IdentityResult> UpdateAsync(AppRole role)
        {
            if (!Store.Exists(role.Id))
                throw new InvalidOperationException();

            await Store.UpdateAsync(role);

            return IdentityResult.Success;
        }
    }
}