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
    public class ClientRepository : IGenericRepository<ClientCreate, ClientModel, ClientUpdate, Guid>
    {
        private readonly ContextType _situation;
        private readonly IMapper _convert;
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context, ContextType situation)
        {
            if (context == null)
                throw new NullReferenceException();

            _convert = new MapperConfiguration(x =>
            {
                x.AddProfile<ClientMaps>();
                x.AddProfile<RoleMaps>();
                x.AddExpressionMapping();
            }).CreateMapper();

            _context = context;
            _situation = situation;
        }

        public async Task<AppClientUri> AddUriAsync(AppClientUri clientUri)
        {
            return await Task.FromResult(_context.AppClientUri.Add(clientUri).Entity);
        }

        public async Task<int> Count(Expression<Func<ClientModel, bool>> predicates = null)
        {
            var query = _context.AppClient.AsQueryable();

            if (predicates != null)
                return await query.Where(_convert.MapExpression<Expression<Func<AppClient, bool>>>(predicates)).CountAsync();

            return await query.CountAsync();
        }

        public async Task<ClientModel> CreateAsync(ClientCreate model)
        {
            var entity = _convert.Map<AppClient>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_convert.Map<ClientModel>(result));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppClient.Where(x => x.Id == key).Single();

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

        public Task<IEnumerable<ClientModel>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ClientModel>> GetAsync(Expression<Func<ClientModel, bool>> predicates = null,
            Expression<Func<ClientModel, object>> orders = null,
            Expression<Func<ClientModel, object>> includes = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppClient.AsQueryable();
            var preds = _convert.MapExpression<Expression<Func<AppClient, bool>>>(predicates);
            var ords = _convert.MapExpression<Expression<Func<AppClient, object>>>(orders);
            var incs = _convert.MapExpression<Expression<Func<AppClient, object>>>(includes);

            if (predicates != null)
                query = query.Where(preds);

            if (orders != null)
                query = query.OrderBy(ords)?
                    .Skip(skip.Value)?
                    .Take(take.Value);

            if (includes != null)
                query = query.Include(incs);

            return await Task.FromResult(_convert.Map<IEnumerable<ClientModel>>(query));
        }

        public async Task<IEnumerable<RoleModel>> GetRoleListAsync(Guid key)
        {
            var result = _context.AppRole.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(_convert.Map<IEnumerable<RoleModel>>(result));
        }

        public async Task<IEnumerable<ClientUriModel>> GetUriListAsync(Guid key)
        {
            var result = _context.AppClientUri.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(_convert.Map<IEnumerable<ClientUriModel>>(result));
        }

        public async Task<ClientModel> UpdateAsync(ClientUpdate model)
        {
            var entity = _context.AppClient.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.IssuerId = model.IssuerId;
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.ClientType = model.ClientType;
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = model.Enabled;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_convert.Map<ClientModel>(_context.Update(entity).Entity));
        }
    }
}
