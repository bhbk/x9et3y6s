using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : IGenericRepository<AppRole, Guid>
    {
        private readonly ContextType _situation;
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context, ContextType situation)
        {
            _context = context;
            _situation = situation;
        }

        public async Task<AppRole> CreateAsync(AppRole entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<int> Count(Expression<Func<AppRole, bool>> predicates = null)
        {
            var query = _context.AppRole.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<bool> DeleteAsync(AppRole entity)
        {
            await Task.FromResult(_context.Remove(entity).Entity);

            return true;
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppRole.Any(x => x.Id == key));
        }

        public Task<IQueryable<AppRole>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<AppRole>> GetAsync(Expression<Func<AppRole, bool>> predicates = null,
            Func<IQueryable<AppRole>, IQueryable<AppRole>> orderBy = null,
            Func<IQueryable<AppRole>, IIncludableQueryable<AppRole, object>> includes = null)
        {
            var query = _context.AppRole.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            query.Include("AppUserRole.User").Load();

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<IList<AppUser>> GetUsersListAsync(AppRole role)
        {
            var result = new List<AppUser>();
            var list = _context.AppUserRole.Where(x => x.RoleId == role.Id).AsQueryable();

            if (list == null)
                throw new InvalidOperationException();

            foreach (AppUserRole entry in list)
                result.Add(_context.AppUser.Where(x => x.Id == entry.UserId).Single());

            return await Task.FromResult(result);
        }

        public async Task<AppRole> UpdateAsync(AppRole role)
        {
            var model = _context.AppRole.Where(x => x.Id == role.Id).Single();

            /*
             * only persist certain fields.
             */

            model.Name = role.Name;
            model.Description = role.Description;
            model.Enabled = role.Enabled;
            model.LastUpdated = DateTime.Now;
            model.Immutable = role.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}