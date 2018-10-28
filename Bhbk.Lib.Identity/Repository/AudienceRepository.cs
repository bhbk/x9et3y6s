using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{
    public class AudienceRepository : IGenericRepository<AppAudience, Guid>
    {
        private readonly AppDbContext _context;

        public AudienceRepository(AppDbContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
        }

        public async Task<AppAudienceUri> AddUriAsync(AppAudienceUri audienceUri)
        {
            return await Task.FromResult(_context.AppAudienceUri.Add(audienceUri).Entity);
        }

        public async Task<AppAudience> CreateAsync(AppAudience entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(AppAudience entity)
        {
            await Task.FromResult(_context.Remove(entity).Entity);

            return true;
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppAudience.Any(x => x.Id == key));
        }

        public async Task<AppAudience> GetAsync(Guid key)
        {
            var audience = _context.AppAudience.Where(x => x.Id == key).SingleOrDefault();

            _context.Entry(audience).Collection(x => x.AppRole).Load();

            return await Task.FromResult(audience);
        }

        public Task<IQueryable<AppAudience>> GetAsync(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<AppAudience>> GetAsync(Expression<Func<AppAudience, bool>> predicates = null, 
            Func<IQueryable<AppAudience>, IQueryable<AppAudience>> orderBy = null, 
            Func<IQueryable<AppAudience>, IIncludableQueryable<AppAudience, object>> includes = null, 
            bool tracking = true)
        {
            IQueryable<AppAudience> query = _context.AppAudience.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<IList<AppRole>> GetRoleListAsync(Guid audienceId)
        {
            IList<AppRole> result = new List<AppRole>();
            var roles = _context.AppRole.Where(x => x.AudienceId == audienceId);

            if (roles == null)
                throw new InvalidOperationException();

            foreach (AppRole role in roles)
                result.Add(role);

            return await Task.FromResult(result);
        }

        public async Task<AppAudience> UpdateAsync(AppAudience entity)
        {
            var model = _context.AppAudience.Where(x => x.Id == entity.Id).Single();

            /*
             * only persist certain fields.
             */

            model.ClientId = entity.ClientId;
            model.Name = entity.Name;
            model.Description = entity.Description;
            model.AudienceType = entity.AudienceType;
            model.LastUpdated = DateTime.Now;
            model.Enabled = entity.Enabled;
            model.Immutable = entity.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}
