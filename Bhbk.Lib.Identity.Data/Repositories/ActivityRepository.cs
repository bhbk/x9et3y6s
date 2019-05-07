using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class ActivityRepository : IGenericRepositoryAsync<tbl_Activities, Guid>
    {
        private readonly _DbContext _context;
        private readonly InstanceContext _instance;

        public ActivityRepository(_DbContext context, InstanceContext instance)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Activities, bool>> predicates = null)
        {
            var query = _context.tbl_Activities.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Activities> CreateAsync(tbl_Activities entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Activities.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.tbl_Activities.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_Activities>> GetAsync(Expression<Func<tbl_Activities, bool>> predicates = null,
            Func<IQueryable<tbl_Activities>, IIncludableQueryable<tbl_Activities, object>> includes = null,
            Func<IQueryable<tbl_Activities>, IOrderedQueryable<tbl_Activities>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Activities.AsQueryable();

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

        public Task<tbl_Activities> UpdateAsync(tbl_Activities entity)
        {
            throw new NotImplementedException();
        }
    }
}
