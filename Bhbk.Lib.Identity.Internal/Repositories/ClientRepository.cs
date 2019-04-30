using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class ClientRepository : IGenericRepositoryAsync<ClientCreate, tbl_Clients, Guid>
    {
        private readonly InstanceContext _instance;
        private readonly IMapper _mapper;
        private readonly IConfiguration _conf;
        private readonly IdentityDbContext _context;

        public ClientRepository(IdentityDbContext context, InstanceContext instance, IMapper mapper, IConfiguration conf)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
            _mapper = mapper;
            _conf = conf;
        }

        public async Task<bool> AccessFailedAsync(Guid key)
        {
            var entity = _context.tbl_Clients.Where(x => x.Id == key).SingleOrDefault();

            entity.LastLoginFailure = DateTime.Now;
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

        public async Task<tbl_Clients> CreateAsync(ClientCreate model)
        {
            var entity = _mapper.Map<tbl_Clients>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<tbl_Urls> CreateUriAsync(UrlCreate model)
        {
            var entity = _mapper.Map<tbl_Urls>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Clients.Where(x => x.Id == key).Single();

            var activity = _context.tbl_Activities.Where(x => x.ClientId == key);
            var roles = _context.tbl_Roles.Where(x => x.ClientId == key);
            var refreshes = _context.tbl_Refreshes.Where(x => x.ClientId == key);
            var states = _context.tbl_States.Where(x => x.ClientId == key);

            try
            {
                _context.RemoveRange(activity);
                _context.RemoveRange(roles);
                _context.RemoveRange(refreshes);
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

        public async Task<ClaimsPrincipal> GenerateAccessTokenAsync(tbl_Clients model)
        {
            var claims = new List<Claim>();

            //defaults...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Id.ToString()));

            //service identity vs. a user identity
            claims.Add(new Claim(ClaimTypes.System, ClientType.server.ToString()));

            foreach (var role in (await GetRolesAsync(model.Id)).ToList().OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            foreach (var claim in (await GetClaimsAsync(model.Id)).ToList().OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, RandomValues.CreateBase64String(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:ClientCredTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<ClaimsPrincipal> GenerateRefreshTokenAsync(tbl_Clients model)
        {
            var claims = new List<Claim>();

            //defaults...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, RandomValues.CreateBase64String(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:ClientCredRefreshExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

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

            //query = query.Include(x => x.Roles);

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

        public async Task<IEnumerable<tbl_Urls>> GetUrisAsync(Guid key)
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
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = model.Enabled;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
