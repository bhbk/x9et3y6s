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
    public class LoginRepository : IGenericRepository<LoginCreate, AppLogin, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public LoginRepository(AppDbContext context, ExecutionType situation, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _mapper = mapper;
        }

        public async Task<int> CountAsync(Expression<Func<AppLogin, bool>> predicates = null)
        {
            var query = _context.AppLogin.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppLogin> CreateAsync(LoginCreate model)
        {
            var entity = _mapper.Map<AppLogin>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<AppLogin>(result));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppLogin.Where(x => x.Id == key).Single();

            try
            {
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
            return await Task.FromResult(_context.AppLogin.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<AppLogin>> GetAsync(Expression<Func<AppLogin, bool>> predicates = null, 
            Func<IQueryable<AppLogin>, IIncludableQueryable<AppLogin, object>> includes = null, 
            Func<IQueryable<AppLogin>, IOrderedQueryable<AppLogin>> orders = null, 
            int? skip = null, 
            int? take = null)
        {
            var query = _context.AppLogin.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.AppUserLogin);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

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

        public async Task<AppLogin> UpdateAsync(AppLogin model)
        {
            var entity = _context.AppLogin.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_mapper.Map<AppLogin>(_context.Update(entity).Entity));
        }
    }
}
