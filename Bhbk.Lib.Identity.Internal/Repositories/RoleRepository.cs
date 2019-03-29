using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : IGenericRepository<RoleCreate, AppRole, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context, ExecutionType situation, IMapper transform)
        {
            _context = context;
            _situation = situation;
            _transform = transform;
        }

        public async Task<int> CountAsync(Expression<Func<AppRole, bool>> predicates = null)
        {
            var query = _context.AppRole.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppRole> CreateAsync(RoleCreate model)
        {
            var entity = _transform.Map<AppRole>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppRole.Where(x => x.Id == key).Single();

            try
            {
                await Task.FromResult(_context.Remove(entity).Entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppRole.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<AppRole>> GetAsync(Expression<Func<AppRole, bool>> predicates = null,
            Func<IQueryable<AppRole>, IIncludableQueryable<AppRole, object>> includes = null,
            Func<IQueryable<AppRole>, IOrderedQueryable<AppRole>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppRole.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.AppUserRole);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IList<AppUser>> GetUsersListAsync(Guid key)
        {
            var result = new List<AppUser>();
            var list = _context.AppUserRole.Where(x => x.RoleId == key).AsQueryable();

            if (list == null)
                throw new InvalidOperationException();

            foreach (AppUserRole entry in list)
                result.Add(_context.AppUser.Where(x => x.Id == entry.UserId).Single());

            return await Task.FromResult(result);
        }

        public async Task<AppRole> UpdateAsync(AppRole model)
        {
            var entity = _context.AppRole.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Enabled = model.Enabled;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_transform.Map<AppRole>(_context.Update(entity).Entity));
        }
    }
}