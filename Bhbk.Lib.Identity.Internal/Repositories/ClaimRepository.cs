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
    public class ClaimRepository : IGenericRepository<ClaimCreate, TClaims, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly DatabaseContext _context;

        public ClaimRepository(DatabaseContext context, ExecutionType situation, IMapper transform)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _transform = transform;
        }

        public async Task<int> CountAsync(Expression<Func<TClaims, bool>> predicates = null)
        {
            var query = _context.TClaims.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<TClaims> CreateAsync(ClaimCreate entity)
        {
            var model = _transform.Map<TClaims>(entity);
            var create = _context.Add(model).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TClaims.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.TClaims.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<TClaims>> GetAsync(Expression<Func<TClaims, bool>> predicates = null,
            Func<IQueryable<TClaims>, IIncludableQueryable<TClaims, object>> includes = null,
            Func<IQueryable<TClaims>, IOrderedQueryable<TClaims>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.TClaims.AsQueryable();

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

        public async Task<TClaims> UpdateAsync(TClaims entity)
        {
            var model = _context.TClaims.Where(x => x.Id == entity.Id).Single();

            /*
             * only persist certain fields.
             */

            model.Subject = entity.Subject;
            model.Type = entity.Type;
            model.Value = entity.Value;
            model.ValueType = entity.ValueType;
            model.LastUpdated = DateTime.Now;
            model.Immutable = entity.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}
