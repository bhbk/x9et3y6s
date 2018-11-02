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
    public class ActivityRepository : IGenericRepository<AppActivity, Guid>
    {
        private readonly AppDbContext _context;

        public ActivityRepository(AppDbContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
        }

        public async Task<int> Count(Expression<Func<AppActivity, bool>> predicates = null)
        {
            IQueryable<AppActivity> query = _context.AppActivity.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppActivity> CreateAsync(AppActivity entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(AppActivity entity)
        {
            await Task.FromResult(_context.Remove(entity).Entity);

            return true;
        }

        public Task<bool> ExistsAsync(Guid key)
        {
            throw new NotImplementedException();
        }

        public Task<AppActivity> GetAsync(Guid key)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<AppActivity>> GetAsync(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<AppActivity>> GetAsync(Expression<Func<AppActivity, bool>> predicates = null, 
            Func<IQueryable<AppActivity>, IQueryable<AppActivity>> orderBy = null, 
            Func<IQueryable<AppActivity>, IIncludableQueryable<AppActivity, object>> includes = null, 
            bool tracking = true)
        {
            IQueryable<AppActivity> query = _context.AppActivity.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public Task<AppActivity> UpdateAsync(AppActivity entity)
        {
            throw new NotImplementedException();
        }
    }
}
