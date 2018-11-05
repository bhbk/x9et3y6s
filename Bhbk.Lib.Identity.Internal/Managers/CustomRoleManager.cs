using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1

namespace Bhbk.Lib.Identity.Managers
{
    public partial class CustomRoleManager : RoleManager<AppRole>
    {
        public readonly CustomRoleStore Store;

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

            return await Store.DeleteAsync(role);
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

        public IList<AppUser> GetUsersListAsync(AppRole role)
        {
            if (!Store.Exists(role.Id))
                throw new InvalidOperationException();

            var result = new List<AppUser>();
            var list = Store.GetUsersAsync(role);

            if (list == null)
                throw new ArgumentNullException();

            foreach (AppUser entry in list)
                result.Add(entry);

            return result;
        }

        public override async Task<IdentityResult> UpdateAsync(AppRole role)
        {
            if (!Store.Exists(role.Id))
                throw new InvalidOperationException();

            return await Store.UpdateAsync(role);
        }
    }
}