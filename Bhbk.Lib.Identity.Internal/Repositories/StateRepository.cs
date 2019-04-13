﻿using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Internal.Models;
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
    public class StateRepository : IGenericRepository<StateCreate, tbl_States, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _shape;
        private readonly IdentityDbContext _context;

        public StateRepository(IdentityDbContext context, ExecutionType situation, IMapper shape)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _shape = shape;
        }

        public async Task<tbl_States> CreateAsync(StateCreate model)
        {
            var entity = _shape.Map<tbl_States>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Refreshes.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.tbl_States.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_States>> GetAsync(Expression<Func<tbl_States, bool>> predicates = null, 
            Func<IQueryable<tbl_States>, IIncludableQueryable<tbl_States, object>> includes = null, 
            Func<IQueryable<tbl_States>, IOrderedQueryable<tbl_States>> orders = null, 
            int? skip = null, 
            int? take = null)
        {
            var query = _context.tbl_States.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<tbl_States> UpdateAsync(tbl_States model)
        {
            var entity = _context.tbl_States.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.LastPolling = model.LastPolling;
            entity.StateConsume = model.StateConsume;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
