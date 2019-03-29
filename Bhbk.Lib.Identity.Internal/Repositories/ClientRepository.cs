using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
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
    public class ClientRepository : IGenericRepository<ClientCreate, AppClient, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IConfigurationRoot _conf;
        private readonly IMapper _transform;
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context, ExecutionType situation, IConfigurationRoot conf, IMapper transform)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _conf = conf;
            _transform = transform;
        }

        public async Task<int> CountAsync(Expression<Func<AppClient, bool>> predicates = null)
        {
            var query = _context.AppClient.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppClient> CreateAsync(ClientCreate model)
        {
            var entity = _transform.Map<AppClient>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<AppClientRefresh> CreateRefreshAsync(ClientRefreshCreate model)
        {
            var entity = _transform.Map<AppClientRefresh>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<AppClientUri> CreateUriAsync(ClientUriCreate model)
        {
            var entity = _transform.Map<AppClientUri>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppClient.Where(x => x.Id == key).Single();

            var roles = _context.AppRole.Where(x => x.ClientId == key);

            try
            {
                _context.RemoveRange(roles);

                await Task.FromResult(_context.Remove(entity).Entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppClient.Any(x => x.Id == key));
        }

        public async Task<ClaimsPrincipal> GenerateAccessTokenAsync(AppClient client)
        {
            /*
             * moving away from microsoft constructs for identity implementation because of un-needed additional 
             * layers of complexity, and limitations, for the simple operations needing to be performed.
             * 
             * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iuserclaimsprincipalfactory-1
             */

            var claims = new List<Claim>();

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:AccessTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<ClaimsPrincipal> GenerateRefreshTokenAsync(AppClient client)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:RefreshTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<IEnumerable<AppClient>> GetAsync(Expression<Func<AppClient, bool>> predicates = null,
            Func<IQueryable<AppClient>, IIncludableQueryable<AppClient, object>> includes = null,
            Func<IQueryable<AppClient>, IOrderedQueryable<AppClient>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppClient.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.AppRole);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<AppRole>> GetRoleListAsync(Guid key)
        {
            var result = _context.AppRole.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<AppClientUri>> GetUriListAsync(Guid key)
        {
            var result = _context.AppClientUri.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(result);
        }

        public async Task<bool> RemoveRefreshTokenAsync(Guid key)
        {
            var token = _context.AppClientRefresh.Where(x => x.Id == key);

            if (token == null)
                throw new ArgumentNullException();

            _context.AppClientRefresh.RemoveRange(token);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveRefreshTokensAsync(AppClient client)
        {
            var tokens = _context.AppClientRefresh.Where(x => x.ClientId == client.Id);

            if (tokens == null)
                throw new ArgumentNullException();

            _context.AppClientRefresh.RemoveRange(tokens);

            return await Task.FromResult(true);
        }

        public async Task<AppClient> UpdateAsync(AppClient model)
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

            return await Task.FromResult(_transform.Map<AppClient>(_context.Update(entity).Entity));
        }
    }
}
