using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Internal.Models;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class RefreshRepository : IGenericRepository<RefreshCreate, tbl_Refreshes, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly IdentityDbContext _context;

        public RefreshRepository(IdentityDbContext context, ExecutionType situation, IMapper transform)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _transform = transform;
        }

        public async Task<tbl_Refreshes> CreateAsync(RefreshCreate model)
        {
            var entity = _transform.Map<tbl_Refreshes>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Refreshes.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.tbl_Refreshes.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_Refreshes>> GetAsync(Expression<Func<tbl_Refreshes, bool>> predicates = null,
            Func<IQueryable<tbl_Refreshes>, IIncludableQueryable<tbl_Refreshes, object>> includes = null,
            Func<IQueryable<tbl_Refreshes>, IOrderedQueryable<tbl_Refreshes>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Refreshes.AsQueryable();

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

        public Task<tbl_Refreshes> UpdateAsync(tbl_Refreshes model)
        {
            throw new NotImplementedException();
        }
    }
}
