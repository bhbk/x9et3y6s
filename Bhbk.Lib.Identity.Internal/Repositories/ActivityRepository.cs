using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class ActivityRepository : IGenericRepositoryAsync<ActivityCreate, tbl_Activities, Guid>
    {
        private readonly InstanceContext _instance;
        private readonly IMapper _mapper;
        private readonly IdentityDbContext _context;

        public ActivityRepository(IdentityDbContext context, InstanceContext instance, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _instance = instance;
            _mapper = mapper;
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Activities, bool>> predicates = null)
        {
            var query = _context.tbl_Activities.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Activities> CreateAsync(ActivityCreate model)
        {
            var entity = _mapper.Map<tbl_Activities>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
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
