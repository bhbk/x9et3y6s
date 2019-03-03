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
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : IGenericRepository<RoleCreate, RoleModel, Guid>
    {
        private readonly ContextType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context, ContextType situation, IMapper mapper)
        {
            _context = context;
            _situation = situation;
            _mapper = mapper;
        }

        public async Task<int> Count(Expression<Func<RoleModel, bool>> predicates = null)
        {
            var query = _context.AppRole.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppRole, bool>>>(predicates);
                return await query.Where(preds).CountAsync();
            }

            return await query.CountAsync();
        }

        public async Task<RoleModel> CreateAsync(RoleCreate model)
        {
            var entity = _mapper.Map<AppRole>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<RoleModel>(result));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppRole.Where(x => x.Id == key).Single();

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

        public Task<IEnumerable<RoleModel>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<RoleModel>> GetAsync(Expression<Func<RoleModel, bool>> predicates = null,
            Expression<Func<RoleModel, object>> orders = null,
            Expression<Func<RoleModel, object>> includes = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppRole.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppRole, bool>>>(predicates);
                query = query.Where(preds);
            }

            if (orders != null)
            {
                var ords = _mapper.MapExpression<Expression<Func<AppRole, object>>>(orders);
                query = query.OrderBy(ords)?
                        .Skip(skip.Value)?
                        .Take(take.Value);
            }

            query.Include("AppUserRole.User").Load();

            //if (includes != null)
            //{
            //    var incs = _mapper.MapExpression<Expression<Func<AppRole, object>>>(includes);
            //    query = query.Include(incs);
            //}

            return await Task.FromResult(_mapper.Map<IEnumerable<RoleModel>>(query));
        }

        public async Task<IList<AppUser>> GetUsersListAsync(RoleModel role)
        {
            var result = new List<AppUser>();
            var list = _context.AppUserRole.Where(x => x.RoleId == role.Id).AsQueryable();

            if (list == null)
                throw new InvalidOperationException();

            foreach (AppUserRole entry in list)
                result.Add(_context.AppUser.Where(x => x.Id == entry.UserId).Single());

            return await Task.FromResult(result);
        }

        public async Task<RoleModel> UpdateAsync(RoleModel model)
        {
            var entity = _context.AppRole.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Enabled = model.Enabled;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_mapper.Map<RoleModel>(_context.Update(entity).Entity));
        }
    }
}