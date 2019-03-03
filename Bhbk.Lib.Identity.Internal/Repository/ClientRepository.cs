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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repository
{
    public class ClientRepository : IGenericRepository<ClientCreate, ClientModel, Guid>
    {
        private readonly ContextType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context, ContextType situation, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _mapper = mapper;
        }

        public async Task<AppClientUri> AddUriAsync(AppClientUri clientUri)
        {
            return await Task.FromResult(_context.AppClientUri.Add(clientUri).Entity);
        }

        public async Task<int> Count(Expression<Func<ClientModel, bool>> predicates = null)
        {
            var query = _context.AppClient.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppClient, bool>>>(predicates);
                return await query.Where(preds).CountAsync();
            }

            return await query.CountAsync();
        }

        public async Task<ClientModel> CreateAsync(ClientCreate model)
        {
            var entity = _mapper.Map<AppClient>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<ClientModel>(result));
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

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppClient, bool>>>(predicates);
                query = query.Where(preds);
            }

            if (orders != null)
            {
                var ords = _mapper.MapExpression<Expression<Func<AppClient, object>>>(orders);
                query = query.OrderBy(ords)?
                        .Skip(skip.Value)?
                        .Take(take.Value);
            }

            query = query.Include("AppRole");

            //if (includes != null)
            //{
            //    var incs = _mapper.MapExpression<Expression<Func<AppClient, object>>>(includes);
            //    query = query.Include(incs);
            //}

            return await Task.FromResult(_mapper.Map<IEnumerable<ClientModel>>(query));
        }

        public async Task<IEnumerable<RoleModel>> GetRoleListAsync(Guid key)
        {
            var result = _context.AppRole.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(_mapper.Map<IEnumerable<RoleModel>>(result));
        }

        public async Task<IEnumerable<ClientUriModel>> GetUriListAsync(Guid key)
        {
            var result = _context.AppClientUri.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(_mapper.Map<IEnumerable<ClientUriModel>>(result));
        }

        public async Task<ClientModel> UpdateAsync(ClientModel model)
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

            return await Task.FromResult(_mapper.Map<ClientModel>(_context.Update(entity).Entity));
        }
    }
}
