﻿using Bhbk.Lib.Core.Interfaces;
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
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : IGenericRepositoryAsync<tbl_Roles, Guid>
    {
        private readonly _DbContext _context;
        private readonly InstanceContext _instance;

        public RoleRepository(_DbContext context, InstanceContext instance)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Roles, bool>> predicates = null)
        {
            var query = _context.tbl_Roles.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Roles> CreateAsync(tbl_Roles entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Roles.Where(x => x.Id == key).Single();

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
            return await Task.FromResult(_context.tbl_Roles.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_Roles>> GetAsync(Expression<Func<tbl_Roles, bool>> predicates = null,
            Func<IQueryable<tbl_Roles>, IIncludableQueryable<tbl_Roles, object>> includes = null,
            Func<IQueryable<tbl_Roles>, IOrderedQueryable<tbl_Roles>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Roles.AsQueryable();

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

        public async Task<IList<tbl_Users>> GetUsersListAsync(Guid key)
        {
            var result = new List<tbl_Users>();
            var entities = _context.tbl_UserRoles.Where(x => x.RoleId == key).AsQueryable();

            if (entities == null)
                throw new InvalidOperationException();

            foreach (var entry in entities)
                result.Add(_context.tbl_Users.Where(x => x.Id == entry.UserId).Single());

            return await Task.FromResult(result);
        }

        public async Task<tbl_Roles> UpdateAsync(tbl_Roles model)
        {
            var entity = _context.tbl_Roles.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Enabled = model.Enabled;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}