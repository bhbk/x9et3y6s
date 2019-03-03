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
    public class IssuerRepository : IGenericRepository<IssuerCreate, IssuerModel, Guid>
    {
        private readonly ContextType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly string _salt;

        public IssuerRepository(AppDbContext context, ContextType situation, IMapper mapper, string salt)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _mapper = mapper;
            _salt = salt;
        }

        public string Salt
        {
            get
            {
                return _salt;
            }
        }

        public async Task<int> Count(Expression<Func<IssuerModel, bool>> predicates = null)
        {
            var query = _context.AppIssuer.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppIssuer, bool>>>(predicates);
                return await query.Where(preds).CountAsync();
            }

            return await query.CountAsync();
        }

        public async Task<IssuerModel> CreateAsync(IssuerCreate model)
        {
            var entity = _mapper.Map<AppIssuer>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<IssuerModel>(result));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppIssuer.Where(x => x.Id == key).Single();

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

        public Task<IEnumerable<IssuerModel>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IssuerModel>> GetAsync(Expression<Func<IssuerModel, bool>> predicates = null,
            Expression<Func<IssuerModel, object>> orders = null,
            Expression<Func<IssuerModel, object>> includes = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppIssuer.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppIssuer, bool>>>(predicates);
                query = query.Where(preds);
            }

            if (orders != null)
            {
                var ords = _mapper.MapExpression<Expression<Func<AppIssuer, object>>>(orders);
                query = query.OrderBy(ords)?
                        .Skip(skip.Value)?
                        .Take(take.Value);
            }

            query = query.Include("AppClient");

            //if (includes != null)
            //{
            //    var incs = _mapper.MapExpression<Expression<Func<AppIssuer, object>>>(includes);
            //    query = query.Include(incs);
            //}

            return await Task.FromResult(_mapper.Map<IEnumerable<IssuerModel>>(query));
        }

        public async Task<IEnumerable<ClientModel>> GetClientsAsync(Guid key)
        {
            var result = _context.AppClient.Where(x => x.IssuerId == key).AsQueryable();

            return await Task.FromResult(_mapper.Map<IEnumerable<ClientModel>>(result));
        }

        public async Task<IssuerModel> UpdateAsync(IssuerModel model)
        {
            var entity = _context.AppIssuer.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.IssuerKey = model.IssuerKey;
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = model.Enabled;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_mapper.Map<IssuerModel>(_context.Update(entity).Entity));
        }
    }
}
