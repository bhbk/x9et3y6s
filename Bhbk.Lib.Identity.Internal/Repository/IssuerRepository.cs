using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{
    public class IssuerRepository : IGenericRepository<AppIssuer, Guid>
    {
        private readonly ContextType _situation;
        private readonly AppDbContext _context;
        private readonly string _salt;

        public IssuerRepository(AppDbContext context, ContextType situation, string salt)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _salt = salt;
        }

        public string Salt
        {
            get
            {
                return _salt;
            }
        }

        public async Task<int> Count(Expression<Func<AppIssuer, bool>> predicates = null)
        {
            var query = _context.AppIssuer.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppIssuer> CreateAsync(AppIssuer entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(AppIssuer entity)
        {
            await Task.FromResult(_context.Remove(entity).Entity);

            return true;
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppClient.Any(x => x.Id == key));
        }

        public Task<IQueryable<AppIssuer>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<AppIssuer>> GetAsync(Expression<Func<AppIssuer, bool>> predicates = null, 
            Func<IQueryable<AppIssuer>, IQueryable<AppIssuer>> orderBy = null, 
            Func<IQueryable<AppIssuer>, IIncludableQueryable<AppIssuer, object>> includes = null)
        {
            var query = _context.AppIssuer.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<IQueryable<AppClient>> GetClientsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppClient.Where(x => x.IssuerId == key).AsQueryable());
        }

        public async Task<AppIssuer> UpdateAsync(AppIssuer entity)
        {
            var model = _context.AppIssuer.Where(x => x.Id == entity.Id).Single();

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
