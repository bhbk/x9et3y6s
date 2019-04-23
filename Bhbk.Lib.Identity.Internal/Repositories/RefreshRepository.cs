using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class RefreshRepository : IGenericRepositoryAsync<RefreshCreate, tbl_Refreshes, Guid>
    {
        private readonly InstanceContext _instance;
        private readonly IMapper _mapper;
        private readonly IdentityDbContext _context;

        public RefreshRepository(IdentityDbContext context, InstanceContext instance, IMapper mapper)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
            _mapper = mapper;
        }

        public async Task<tbl_Refreshes> CreateAsync(RefreshCreate model)
        {
            var entity = _mapper.Map<tbl_Refreshes>(model);
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
