using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repository
{
    public class LoginRepository : IGenericRepository<LoginCreate, LoginModel, Guid>
    {
        private readonly ContextType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public LoginRepository(AppDbContext context, ContextType situation, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _mapper = mapper;
        }

        public async Task<int> Count(Expression<Func<LoginModel, bool>> predicates = null)
        {
            var query = _context.AppLogin.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppLogin, bool>>>(predicates);
                return await query.Where(preds).CountAsync();
            }

            return await query.CountAsync();
        }

        public async Task<LoginModel> CreateAsync(LoginCreate model)
        {
            var entity = _mapper.Map<AppLogin>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<LoginModel>(result));
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

        public Task<bool> ExistsAsync(Guid key)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LoginModel>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<LoginModel>> GetAsync(Expression<Func<LoginModel, bool>> predicates = null,
            Expression<Func<LoginModel, object>> orders = null,
            Expression<Func<LoginModel, object>> includes = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppLogin.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppLogin, bool>>>(predicates);
                query = query.Where(preds);
            }

            if (orders != null)
            {
                var ords = _mapper.MapExpression<Expression<Func<AppLogin, object>>>(orders);
                query = query.OrderBy(ords)?
                        .Skip(skip.Value)?
                        .Take(take.Value);
            }

            query = query.Include("AppUserLogin.User");

            //if (includes != null)
            //{
            //    var incs = _mapper.MapExpression<Expression<Func<AppLogin, object>>>(includes);
            //    query = query.Include(incs);
            //}

            return await Task.FromResult(_mapper.Map<IEnumerable<LoginModel>>(query));
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

        public async Task<LoginModel> UpdateAsync(LoginModel model)
        {
            var entity = _context.AppLogin.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.LoginProvider = model.LoginProvider;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_mapper.Map<LoginModel>(_context.Update(entity).Entity));
        }
    }
}
