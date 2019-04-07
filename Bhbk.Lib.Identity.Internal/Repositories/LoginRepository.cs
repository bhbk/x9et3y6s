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
    public class LoginRepository : IGenericRepository<LoginCreate, TLogins, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _transform;
        private readonly _DbContext _context;

        public LoginRepository(_DbContext context, ExecutionType situation, IMapper transform)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _transform = transform;
        }

        public async Task<int> CountAsync(Expression<Func<TLogins, bool>> predicates = null)
        {
            var query = _context.TLogins.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<TLogins> CreateAsync(LoginCreate entity)
        {
            var model = _transform.Map<TLogins>(entity);
            var create = _context.Add(model).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TLogins.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.TLogins.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<TLogins>> GetAsync(Expression<Func<TLogins, bool>> predicates = null,
            Func<IQueryable<TLogins>, IIncludableQueryable<TLogins, object>> includes = null,
            Func<IQueryable<TLogins>, IOrderedQueryable<TLogins>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.TLogins.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.UserLogins);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IQueryable<TUsers>> GetUsersAsync(Guid key)
        {
            var result = (IList<string>)_context.TLogins
                .Join(_context.TUserLogins, x => x.Id, y => y.LoginId, (login1, user1) => new {
                    LoginId = login1.Id,
                    UserId = user1.UserId
                })
                .Where(x => x.LoginId == key)
                .Select(x => x.UserId.ToString().ToLower())
                .Distinct()
                .ToList();

            return await Task.FromResult(_context.TUsers.Where(x => result.Contains(x.Id.ToString())));
        }

        public async Task<TLogins> UpdateAsync(TLogins entity)
        {
            var model = _context.TLogins.Where(x => x.Id == entity.Id).Single();

            /*
             * only persist certain fields.
             */

            model.Name = entity.Name;
            model.Description = entity.Description;
            model.LoginKey = entity.LoginKey;
            model.LastUpdated = DateTime.Now;
            model.Enabled = entity.Enabled;
            model.Immutable = entity.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }
    }
}
