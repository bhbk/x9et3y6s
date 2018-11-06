using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using Bhbk.Lib.Identity.Maps;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{
    public class ActivityRepository : IGenericRepository<ActivityCreate, ActivityModel, Guid>
    {
        private readonly ContextType _situation;
        private readonly IMapper _convert;
        private readonly AppDbContext _context;

        public ActivityRepository(AppDbContext context, ContextType situation)
        {
            if (context == null)
                throw new NullReferenceException();

            _convert = new MapperConfiguration(x =>
            {
                x.AddProfile<ActivityMaps>();
                x.AddExpressionMapping();
            }).CreateMapper();

            _context = context;
            _situation = situation;
        }

        public async Task<int> Count(Expression<Func<ActivityModel, bool>> predicates = null)
        {
            var query = _context.AppActivity.AsQueryable();

            if (predicates != null)
                return await query.Where(_convert.MapExpression<Expression<Func<AppActivity, bool>>>(predicates)).CountAsync();

            return await query.CountAsync();
        }

        public async Task<ActivityModel> CreateAsync(ActivityCreate model)
        {
            var entity = _convert.Map<AppActivity>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_convert.Map<ActivityModel>(result));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppActivity.Where(x => x.Id == key).Single();

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

        public Task<IEnumerable<ActivityModel>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ActivityModel>> GetAsync(Expression<Func<ActivityModel, bool>> predicates = null,
            Expression<Func<ActivityModel, object>> orders = null,
            Expression<Func<ActivityModel, object>> includes = null,
            int? skip = null, 
            int? take = null)
        {
            var query = _context.AppActivity.AsQueryable();
            var preds = _convert.MapExpression<Expression<Func<AppActivity, bool>>>(predicates);
            var ords = _convert.MapExpression<Expression<Func<AppActivity, object>>>(orders);
            var incs = _convert.MapExpression<Expression<Func<AppActivity, object>>>(includes);

            if (predicates != null)
                query = query.Where(preds);

            if (orders != null)
                query = query.OrderBy(ords)?
                    .Skip(skip.Value)?
                    .Take(take.Value);

            if (includes != null)
                query = query.Include(incs);

            return await Task.FromResult(_convert.Map<IEnumerable<ActivityModel>>(query));
        }
    }
}
