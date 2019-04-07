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

    public class RoleRepository : IGenericRepository<RoleCreate, TRoles, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly _DbContext _context;

        public RoleRepository(_DbContext context, ExecutionType situation, IMapper transform)
        {
            _context = context;
            _situation = situation;
            _transform = transform;
        }

        public async Task<int> CountAsync(Expression<Func<TRoles, bool>> predicates = null)
        {
            var query = _context.TRoles.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<TRoles> CreateAsync(RoleCreate entity)
        {
            var model = _transform.Map<TRoles>(entity);
            var create = _context.Add(model).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TRoles.Where(x => x.Id == key).Single();

            try
            {
                _context.Remove(entity);

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.TRoles.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<TRoles>> GetAsync(Expression<Func<TRoles, bool>> predicates = null,
            Func<IQueryable<TRoles>, IIncludableQueryable<TRoles, object>> includes = null,
            Func<IQueryable<TRoles>, IOrderedQueryable<TRoles>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.TRoles.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.UserRoles);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IList<TUsers>> GetUsersListAsync(Guid key)
        {
            var result = new List<TUsers>();
            var entities = _context.TUserRoles.Where(x => x.RoleId == key).AsQueryable();

            if (entities == null)
                throw new InvalidOperationException();

            foreach (var entry in entities)
                result.Add(_context.TUsers.Where(x => x.Id == entry.UserId).Single());

            return await Task.FromResult(result);
        }

        public async Task<TRoles> UpdateAsync(TRoles entity)
        {
            var model = _context.TRoles.Where(x => x.Id == entity.Id).Single();

            /*
             * only persist certain fields.
             */

            model.Name = entity.Name;
            model.Description = entity.Description;
            model.Enabled = entity.Enabled;
            model.LastUpdated = DateTime.Now;
            model.Immutable = entity.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}