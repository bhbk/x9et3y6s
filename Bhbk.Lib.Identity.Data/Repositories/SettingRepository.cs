using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class SettingRepository : IGenericRepositoryAsync<tbl_Settings, Guid>
    {
        private readonly _DbContext _context;
        private readonly InstanceContext _instance;

        public SettingRepository(_DbContext context, InstanceContext instance)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
        }

        public async Task<tbl_Settings> CreateAsync(tbl_Settings entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Settings.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.tbl_Settings.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_Settings>> GetAsync(Expression<Func<tbl_Settings, bool>> predicates = null, 
            Func<IQueryable<tbl_Settings>, IIncludableQueryable<tbl_Settings, object>> includes = null, 
            Func<IQueryable<tbl_Settings>, IOrderedQueryable<tbl_Settings>> orders = null, 
            int? skip = null, 
            int? take = null)
        {
            var query = _context.tbl_Settings.AsQueryable();

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

        public async Task<tbl_Settings> UpdateAsync(tbl_Settings model)
        {
            var entity = _context.tbl_Settings.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.ConfigKey = model.ConfigKey;
            entity.ConfigValue = model.ConfigValue;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
