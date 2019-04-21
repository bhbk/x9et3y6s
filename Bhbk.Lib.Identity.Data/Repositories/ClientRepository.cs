using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataAccess.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class ClientRepository : IGenericRepositoryAsync<tbl_Clients, Guid>
    {
        private readonly _DbContext _context;
        private readonly InstanceContext _instance;
        private IClockService _clock;

        public ClientRepository(_DbContext context, InstanceContext instance)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
            _clock = new ClockService(_instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public async Task<bool> AccessFailedAsync(Guid key)
        {
            var entity = _context.tbl_Clients.Where(x => x.Id == key).SingleOrDefault();

            entity.LastLoginFailure = Clock.UtcDateTime;
            entity.AccessFailedCount++;

            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> AccessSuccessAsync(Guid key)
        {
            var entity = _context.tbl_Clients.Where(x => x.Id == key).Single();

            entity.LastLoginSuccess = DateTime.Now;
            entity.AccessSuccessCount++;

            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Clients, bool>> predicates = null)
        {
            var query = _context.tbl_Clients.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Clients> CreateAsync(tbl_Clients entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<tbl_Urls> CreateUrlAsync(tbl_Urls entity)
        {
            return await Task.FromResult(_context.Add(entity).Entity);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Clients.Where(x => x.Id == key).Single();

            var activity = _context.tbl_Activities.Where(x => x.ClientId == key);
            var roles = _context.tbl_Roles.Where(x => x.ClientId == key);
            var refreshes = _context.tbl_Refreshes.Where(x => x.ClientId == key);
            var settings = _context.tbl_Settings.Where(x => x.ClientId == key);
            var states = _context.tbl_States.Where(x => x.ClientId == key);

            try
            {
                _context.RemoveRange(activity);
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
            return await Task.FromResult(_context.tbl_Clients.Any(x => x.Id == key));
        }

        public async Task<ClaimsPrincipal> GenerateAccessClaimsAsync(tbl_Issuers issuer, tbl_Clients client)
        {
            var expire = _context.tbl_Settings.Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingAccessExpire).Single();

            var claims = new List<Claim>();

            //defaults...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

            //service identity vs. a user identity
            claims.Add(new Claim(ClaimTypes.System, ClientType.server.ToString()));

            foreach (var role in (await GetRolesAsync(client.Id)).ToList().OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            foreach (var claim in (await GetClaimsAsync(client.Id)).ToList().OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, Base64.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<ClaimsPrincipal> GenerateRefreshClaimsAsync(tbl_Issuers issuer, tbl_Clients client)
        {
            var expire = _context.tbl_Settings.Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //defaults...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, Base64.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<IEnumerable<tbl_Clients>> GetAsync(Expression<Func<tbl_Clients, bool>> predicates = null,
            Func<IQueryable<tbl_Clients>, IIncludableQueryable<tbl_Clients, object>> includes = null,
            Func<IQueryable<tbl_Clients>, IOrderedQueryable<tbl_Clients>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Clients.AsQueryable();

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

        public async Task<IEnumerable<tbl_Urls>> GetUrlsAsync(Expression<Func<tbl_Urls, bool>> predicates = null,
            Func<IQueryable<tbl_Urls>, IIncludableQueryable<tbl_Urls, object>> includes = null,
            Func<IQueryable<tbl_Urls>, IOrderedQueryable<tbl_Urls>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Urls.AsQueryable();

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

        public async Task<IEnumerable<tbl_Claims>> GetClaimsAsync(Guid key)
        {
            var entity = _context.tbl_Clients.Where(x => x.Id == key).Single();
            var result = new List<tbl_Claims>() { };

            result.Add(new tbl_Claims() { Id = Guid.NewGuid(), Type = ClaimTypes.NameIdentifier, Value = entity.Id.ToString() });

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<tbl_Roles>> GetRolesAsync(Guid key)
        {
            var result = _context.tbl_Roles.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<tbl_Urls>> GetUrlsAsync(Guid key)
        {
            var result = _context.tbl_Urls.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(result);
        }

        public async Task<tbl_Clients> UpdateAsync(tbl_Clients model)
        {
            var entity = _context.tbl_Clients.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.IssuerId = model.IssuerId;
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.ClientType = model.ClientType;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Enabled = model.Enabled;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
