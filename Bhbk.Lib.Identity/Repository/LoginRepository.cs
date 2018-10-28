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
    public class LoginRepository : IGenericRepository<AppLogin, Guid>
    {
        private readonly AppDbContext _context;

        public LoginRepository(AppDbContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
        }

        public async Task<AppLogin> CreateAsync(AppLogin entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(AppLogin entity)
        {
            await Task.FromResult(_context.Remove(entity).Entity);

            return true;
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppLogin.Any(x => x.Id == key));
        }

        public async Task<AppLogin> GetAsync(Guid key)
        {
            return await Task.FromResult(_context.AppLogin.Where(x => x.Id == key).SingleOrDefault());
        }

        public Task<IQueryable<AppLogin>> GetAsync(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<AppLogin>> GetAsync(Expression<Func<AppLogin, bool>> predicates = null,
            Func<IQueryable<AppLogin>, IQueryable<AppLogin>> orderBy = null,
            Func<IQueryable<AppLogin>, IIncludableQueryable<AppLogin, object>> includes = null,
            bool tracking = true)
        {
            IQueryable<AppLogin> query = _context.AppLogin.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<IQueryable<AppUser>> GetUsersAsync(Guid key)
        {
            var result = (IList<string>)_context.AppLogin
                .Join(_context.AppUserLogin, x => x.Id, y => y.LoginId, (login1, user1) => new {
                    LoginId = login1.Id,
                    UserId = user1.UserId
                })
                .Where(x => x.LoginId == key)
                .Select(x => x.UserId.ToString().ToLower())
                .Distinct()
                .ToList();

            return await Task.FromResult(_context.AppUser.Where(x => result.Contains(x.Id.ToString())));
        }

        public async Task<AppLogin> UpdateAsync(AppLogin entity)
        {
            var model = _context.AppLogin.Where(x => x.Id == entity.Id).Single();

            /*
             * only persist certain fields.
             */

            model.LoginProvider = entity.LoginProvider;
            model.Immutable = entity.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}
