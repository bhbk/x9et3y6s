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
    public class StateRepository : IGenericRepository<StateCreate, TStates, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly _DbContext _context;

        public StateRepository(_DbContext context, ExecutionType situation, IMapper transform)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _transform = transform;
        }

        public async Task<TStates> CreateAsync(StateCreate entity)
        {
            var model = _transform.Map<TStates>(entity);
            var create = _context.Add(model).Entity;

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
            return await Task.FromResult(_context.TStates.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<TStates>> GetAsync(Expression<Func<TStates, bool>> predicates = null, 
            Func<IQueryable<TStates>, IIncludableQueryable<TStates, object>> includes = null, 
            Func<IQueryable<TStates>, IOrderedQueryable<TStates>> orders = null, 
            int? skip = null, 
            int? take = null)
        {
            var query = _context.TStates.AsQueryable();

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

        public async Task<TStates> UpdateAsync(TStates entity)
        {
            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
