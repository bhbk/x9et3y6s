using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Core.Repositories;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Models.Admin;
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
    public class LoginRepository : IGenericRepositoryAsync<LoginCreate, tbl_Logins, Guid>
    {
        private readonly InstanceContext _instance;
        private readonly IMapper _mapper;
        private readonly IdentityDbContext _context;

        public LoginRepository(IdentityDbContext context, InstanceContext instance, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _instance = instance;
            _mapper = mapper;
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Logins, bool>> predicates = null)
        {
            var query = _context.tbl_Logins.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Logins> CreateAsync(LoginCreate model)
        {
            var entit = _mapper.Map<tbl_Logins>(model);
            var create = _context.Add(entit).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Logins.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.tbl_Logins.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_Logins>> GetAsync(Expression<Func<tbl_Logins, bool>> predicates = null,
            Func<IQueryable<tbl_Logins>, IIncludableQueryable<tbl_Logins, object>> includes = null,
            Func<IQueryable<tbl_Logins>, IOrderedQueryable<tbl_Logins>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Logins.AsQueryable();

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

        public async Task<IQueryable<tbl_Users>> GetUsersAsync(Guid key)
        {
            var result = (IList<string>)_context.tbl_Logins
                .Join(_context.tbl_UserLogins, x => x.Id, y => y.LoginId, (login1, user1) => new {
                    LoginId = login1.Id,
                    UserId = user1.UserId
                })
                .Where(x => x.LoginId == key)
                .Select(x => x.UserId.ToString())
                .Distinct()
                .ToList();

            return await Task.FromResult(_context.tbl_Users.Where(x => result.Contains(x.Id.ToString())));
        }

        public async Task<tbl_Logins> UpdateAsync(tbl_Logins model)
        {
            var entity = _context.tbl_Logins.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.LoginKey = model.LoginKey;
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = model.Enabled;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
