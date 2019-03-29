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
    public class IssuerRepository : IGenericRepository<IssuerCreate, AppIssuer, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly AppDbContext _context;
        private readonly string _salt;

        public string Salt { get => _salt; }

        public IssuerRepository(AppDbContext context, ExecutionType situation, IMapper transform, string salt)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _transform = transform;
            _salt = salt;
        }

        public async Task<int> CountAsync(Expression<Func<AppIssuer, bool>> predicates = null)
        {
            var query = _context.AppIssuer.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppIssuer> CreateAsync(IssuerCreate model)
        {
            var entity = _transform.Map<AppIssuer>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppIssuer.Where(x => x.Id == key).Single();

            var claims = _context.AppClaim.Where(x => x.IssuerId == key);
            var clients = _context.AppClient.Where(x => x.IssuerId == key);
            var roles = _context.AppRole.Where(x => x.Client.IssuerId == key);

            try
            {
                _context.RemoveRange(claims);
                _context.RemoveRange(clients);
                _context.RemoveRange(roles);

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
            return await Task.FromResult(_context.AppIssuer.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<AppIssuer>> GetAsync(Expression<Func<AppIssuer, bool>> predicates = null,
            Func<IQueryable<AppIssuer>, IIncludableQueryable<AppIssuer, object>> includes = null,
            Func<IQueryable<AppIssuer>, IOrderedQueryable<AppIssuer>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppIssuer.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.AppClient);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<AppClient>> GetClientsAsync(Guid key)
        {
            var result = _context.AppClient.Where(x => x.IssuerId == key).AsQueryable();

            return await Task.FromResult(result);
        }

        public async Task<AppIssuer> UpdateAsync(AppIssuer model)
        {
            var entity = _context.AppIssuer.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.IssuerKey = model.IssuerKey;
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = model.Enabled;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_transform.Map<AppIssuer>(_context.Update(entity).Entity));
        }
    }
}
