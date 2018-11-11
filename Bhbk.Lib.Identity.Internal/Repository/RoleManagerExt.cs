using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1

namespace Bhbk.Lib.Identity.Repository
{
    public partial class RoleManagerExt : RoleManager<AppRole>
    {
        public readonly RoleStoreExt Store;

        public RoleManagerExt(RoleStoreExt store, 
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

        public async Task<int> Count(Expression<Func<AppRole, bool>> predicates = null)
        {
            var query = Store.Context.AppRole.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
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

        public async Task<IQueryable<AppRole>> GetAsync(Expression<Func<AppRole, bool>> predicates = null,
            Func<IQueryable<AppRole>, IQueryable<AppRole>> orderBy = null,
            Func<IQueryable<AppRole>, IIncludableQueryable<AppRole, object>> includes = null,
            bool tracking = true)
        {
            var query = Store.Context.AppRole.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            query.Include("AppUserRole.User").Load();

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
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