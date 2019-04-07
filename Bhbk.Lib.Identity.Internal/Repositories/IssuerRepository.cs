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
    public class IssuerRepository : IGenericRepository<IssuerCreate, TIssuers, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly _DbContext _context;
        private readonly string _salt;

        public string Salt { get => _salt; }

        public IssuerRepository(_DbContext context, ExecutionType situation, IMapper transform, string salt)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _transform = transform;
            _salt = salt;
        }

        public async Task<int> CountAsync(Expression<Func<TIssuers, bool>> predicates = null)
        {
            var query = _context.TIssuers.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<TIssuers> CreateAsync(IssuerCreate entity)
        {
            var model = _transform.Map<TIssuers>(entity);
            var create = _context.Add(model).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TIssuers.Where(x => x.Id == key).Single();

            var claims = _context.TClaims.Where(x => x.IssuerId == key);
            var clients = _context.TClients.Where(x => x.IssuerId == key);
            var roles = _context.TRoles.Where(x => x.Client.IssuerId == key);
            var refreshes = _context.TRefreshes.Where(x => x.IssuerId == key);

            try
            {
                _context.RemoveRange(claims);
                _context.RemoveRange(clients);
                _context.RemoveRange(roles);
                _context.RemoveRange(refreshes);

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
            return await Task.FromResult(_context.TIssuers.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<TIssuers>> GetAsync(Expression<Func<TIssuers, bool>> predicates = null,
            Func<IQueryable<TIssuers>, IIncludableQueryable<TIssuers, object>> includes = null,
            Func<IQueryable<TIssuers>, IOrderedQueryable<TIssuers>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.TIssuers.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.Clients);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<TClients>> GetClientsAsync(Guid key)
        {
            var result = _context.TClients.Where(x => x.IssuerId == key).AsQueryable();

            return await Task.FromResult(result);
        }

        public async Task<TIssuers> UpdateAsync(TIssuers entity)
        {
            var model = _context.TIssuers.Where(x => x.Id == entity.Id).Single();

            /*
             * only persist certain fields.
             */

            model.Name = entity.Name;
            model.Description = entity.Description;
            model.IssuerKey = entity.IssuerKey;
            model.LastUpdated = DateTime.Now;
            model.Enabled = entity.Enabled;
            model.Immutable = entity.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}
