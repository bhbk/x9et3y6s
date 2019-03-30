using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class RefreshRepository : IGenericRepository<RefreshCreate, TRefreshes, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly DatabaseContext _context;

        public RefreshRepository(DatabaseContext context, ExecutionType situation, IMapper transform)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _transform = transform;
        }

        public async Task<TRefreshes> CreateAsync(RefreshCreate model)
        {
            var entity = _transform.Map<TRefreshes>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TRefreshes.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.TRefreshes.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<TRefreshes>> GetAsync(Expression<Func<TRefreshes, bool>> predicates = null, 
            Func<IQueryable<TRefreshes>, IIncludableQueryable<TRefreshes, object>> includes = null, 
            Func<IQueryable<TRefreshes>, IOrderedQueryable<TRefreshes>> orders = null, 
            int? skip = null, 
            int? take = null)
        {
            var query = _context.TRefreshes.AsQueryable();

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

        public Task<TRefreshes> UpdateAsync(TRefreshes entity)
        {
            throw new NotImplementedException();
        }
    }
}
