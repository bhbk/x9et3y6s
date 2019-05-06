using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
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
    public class ClaimRepository : IGenericRepositoryAsync<tbl_Claims, Guid>
    {
        private readonly IdentityDbContext _context;
        private readonly InstanceContext _instance;

        public ClaimRepository(IdentityDbContext context, InstanceContext instance)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Claims, bool>> predicates = null)
        {
            var query = _context.tbl_Claims.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Claims> CreateAsync(tbl_Claims entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Claims.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.tbl_Claims.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_Claims>> GetAsync(Expression<Func<tbl_Claims, bool>> predicates = null,
            Func<IQueryable<tbl_Claims>, IIncludableQueryable<tbl_Claims, object>> includes = null,
            Func<IQueryable<tbl_Claims>, IOrderedQueryable<tbl_Claims>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Claims.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<tbl_Claims> UpdateAsync(tbl_Claims model)
        {
            var entity = _context.tbl_Claims.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Subject = model.Subject;
            entity.Type = model.Type;
            entity.Value = model.Value;
            entity.ValueType = model.ValueType;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
