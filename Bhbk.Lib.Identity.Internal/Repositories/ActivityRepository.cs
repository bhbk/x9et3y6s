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
    public class ActivityRepository : IGenericRepository<ActivityCreate, AppActivity, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ActivityRepository(AppDbContext context, ExecutionType situation, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _mapper = mapper;
        }

        public async Task<int> CountAsync(Expression<Func<AppActivity, bool>> predicates = null)
        {
            var query = _context.AppActivity.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppActivity> CreateAsync(ActivityCreate model)
        {
            var entity = _mapper.Map<AppActivity>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<AppActivity>(result));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppActivity.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.AppActivity.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<AppActivity>> GetAsync(Expression<Func<AppActivity, bool>> predicates = null,
            Func<IQueryable<AppActivity>, IIncludableQueryable<AppActivity, object>> includes = null,
            Func<IQueryable<AppActivity>, IOrderedQueryable<AppActivity>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppActivity.AsQueryable();

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

        public Task<AppActivity> UpdateAsync(AppActivity entity)
        {
            throw new NotImplementedException();
        }
    }
}
