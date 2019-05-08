﻿using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
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
    public class IssuerRepository : IGenericRepositoryAsync<tbl_Issuers, Guid>
    {
        private readonly _DbContext _context;
        private readonly InstanceContext _instance;
        public string Salt { get; }

        public IssuerRepository(_DbContext context, InstanceContext instance, string salt)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;

            Salt = salt;
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Issuers, bool>> predicates = null)
        {
            var query = _context.tbl_Issuers.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Issuers> CreateAsync(tbl_Issuers entity)
        {
            entity.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = entity.Id,
                    ConfigKey = Constants.ApiSettingAccessExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            entity.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = entity.Id,
                    ConfigKey = Constants.ApiSettingRefreshExpire,
                    ConfigValue = 86400.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            entity.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = entity.Id,
                    ConfigKey = Constants.ApiSettingTotpExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            entity.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = entity.Id,
                    ConfigKey = Constants.ApiSettingPollingMax,
                    ConfigValue = 10.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Issuers.Where(x => x.Id == key).Single();

            var claims = _context.tbl_Claims.Where(x => x.IssuerId == key);
            var clients = _context.tbl_Clients.Where(x => x.IssuerId == key);
            var roles = _context.tbl_Roles.Where(x => x.Client.IssuerId == key);
            var refreshes = _context.tbl_Refreshes.Where(x => x.IssuerId == key);
            var settings = _context.tbl_Settings.Where(x => x.IssuerId == key);
            var states = _context.tbl_States.Where(x => x.IssuerId == key);

            try
            {
                _context.RemoveRange(claims);
                _context.RemoveRange(clients);
                _context.RemoveRange(roles);
                _context.RemoveRange(refreshes);
                _context.RemoveRange(settings);
                _context.RemoveRange(states);
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
            return await Task.FromResult(_context.tbl_Issuers.Any(x => x.Id == key));
        }

        public async Task<IEnumerable<tbl_Issuers>> GetAsync(Expression<Func<tbl_Issuers, bool>> predicates = null,
            Func<IQueryable<tbl_Issuers>, IIncludableQueryable<tbl_Issuers, object>> includes = null,
            Func<IQueryable<tbl_Issuers>, IOrderedQueryable<tbl_Issuers>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Issuers.AsQueryable();

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

        public async Task<IEnumerable<tbl_Clients>> GetClientsAsync(Guid key)
        {
            var result = _context.tbl_Clients.Where(x => x.IssuerId == key).AsQueryable();

            return await Task.FromResult(result);
        }

        public async Task<tbl_Issuers> UpdateAsync(tbl_Issuers model)
        {
            var entity = _context.tbl_Issuers.Where(x => x.Id == model.Id).Single();

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

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}