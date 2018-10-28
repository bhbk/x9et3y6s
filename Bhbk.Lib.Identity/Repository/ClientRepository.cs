using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{
    public class ClientRepository : IGenericRepository<AppClient, Guid>
    {
        private readonly AppDbContext _context;
        private string _salt = null;

        public ClientRepository(AppDbContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
        }

        public string Salt
        {
            get
            {
                return _salt;
            }
            set
            {
                _salt = value;
            }
        }

        public async Task<AppClient> CreateAsync(AppClient entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(AppClient entity)
        {
            await Task.FromResult(_context.Remove(entity).Entity);

            return true;
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppAudience.Any(x => x.Id == key));
        }

        public async Task<IQueryable<AppAudience>> GetAudiencesAsync(Guid key)
        {
            return await Task.FromResult(_context.AppAudience.Where(x => x.ClientId == key).AsQueryable());
        }

        public async Task<AppClient> GetAsync(Guid key)
        {
            var client = _context.AppClient.Where(x => x.Id == key).SingleOrDefault();

            _context.Entry(client).Collection(x => x.AppAudience).Load();

            return await Task.FromResult(client);
        }

        public Task<IQueryable<AppClient>> GetAsync(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<AppClient>> GetAsync(Expression<Func<AppClient, bool>> predicates = null, 
            Func<IQueryable<AppClient>, IQueryable<AppClient>> orderBy = null, 
            Func<IQueryable<AppClient>, IIncludableQueryable<AppClient, object>> includes = null, 
            bool tracking = true)
        {
            IQueryable<AppClient> query = _context.AppClient.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<AppClient> UpdateAsync(AppClient entity)
        {
            var model = _context.AppClient.Where(x => x.Id == entity.Id).Single();

            /*
             * only persist certain fields.
             */

            model.Name = entity.Name;
            model.Description = entity.Description;
            model.ClientKey = entity.ClientKey;
            model.LastUpdated = DateTime.Now;
            model.Enabled = entity.Enabled;
            model.Immutable = entity.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}
