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
    public class ClaimRepository : IGenericRepository<ClaimCreate, AppClaim, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ClaimRepository(AppDbContext context, ExecutionType situation, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _mapper = mapper;
        }

        public async Task<int> CountAsync(Expression<Func<AppClaim, bool>> predicates = null)
        {
            var query = _context.AppClaim.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppClaim> CreateAsync(ClaimCreate model)
        {
            var entity = _mapper.Map<AppClaim>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<AppClaim>(result));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppClaim.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.AppClaim.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<AppClaim>> GetAsync(Expression<Func<AppClaim, bool>> predicates = null, 
            Func<IQueryable<AppClaim>, IIncludableQueryable<AppClaim, object>> includes = null, 
            Func<IQueryable<AppClaim>, IOrderedQueryable<AppClaim>> orders = null, 
            int? skip = null, 
            int? take = null)
        {
            var query = _context.AppClaim.AsQueryable();

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

        public async Task<AppClaim> UpdateAsync(AppClaim model)
        {
            var entity = _context.AppClaim.Where(x => x.Id == model.Id).Single();

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

            return await Task.FromResult(_mapper.Map<AppClaim>(_context.Update(entity).Entity));
        }
    }
}
