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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class ActivityRepository : IGenericRepository<ActivityCreate, TActivities, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _mapper;
        private readonly DatabaseContext _context;

        public ActivityRepository(DatabaseContext context, ExecutionType situation, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _mapper = mapper;
        }

        public async Task<int> CountAsync(Expression<Func<TActivities, bool>> predicates = null)
        {
            var query = _context.TActivities.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<TActivities> CreateAsync(ActivityCreate model)
        {
            var entity = _mapper.Map<TActivities>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TActivities.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.TActivities.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<TActivities>> GetAsync(Expression<Func<TActivities, bool>> predicates = null,
            Func<IQueryable<TActivities>, IIncludableQueryable<TActivities, object>> includes = null,
            Func<IQueryable<TActivities>, IOrderedQueryable<TActivities>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.TActivities.AsQueryable();

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

        public Task<TActivities> UpdateAsync(TActivities entity)
        {
            throw new NotImplementedException();
        }
    }
}
