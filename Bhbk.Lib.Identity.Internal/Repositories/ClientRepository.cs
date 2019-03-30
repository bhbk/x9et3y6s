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
    public class ClientRepository : IGenericRepository<ClientCreate, TClients, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IConfigurationRoot _conf;
        private readonly IMapper _transform;
        private readonly DatabaseContext _context;

        public ClientRepository(DatabaseContext context, ExecutionType situation, IConfigurationRoot conf, IMapper transform)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _conf = conf;
            _transform = transform;
        }

        public async Task<int> CountAsync(Expression<Func<TClients, bool>> predicates = null)
        {
            var query = _context.TClients.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<TClients> CreateAsync(ClientCreate model)
        {
            var entity = _transform.Map<TClients>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<TRefreshes> CreateRefreshAsync(RefreshCreate model)
        {
            var entity = _transform.Map<TRefreshes>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<TClientUrls> CreateUriAsync(ClientUriCreate model)
        {
            var entity = _transform.Map<TClientUrls>(model);
            var create = _context.Add(entity).Entity;

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TClients.Where(x => x.Id == key).Single();

            var roles = _context.TRoles.Where(x => x.ClientId == key);
            var refreshes = _context.TRefreshes.Where(x => x.ClientId == key);

            try
            {
                _context.RemoveRange(roles);
                _context.RemoveRange(refreshes);

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
            return await Task.FromResult(_context.TClients.Any(x => x.Id == key));
        }

        public async Task<ClaimsPrincipal> GenerateAccessTokenAsync(TClients client)
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

        public async Task<ClaimsPrincipal> GenerateRefreshTokenAsync(TClients client)
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

        public async Task<IEnumerable<TClients>> GetAsync(Expression<Func<TClients, bool>> predicates = null,
            Func<IQueryable<TClients>, IIncludableQueryable<TClients, object>> includes = null,
            Func<IQueryable<TClients>, IOrderedQueryable<TClients>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.TClients.AsQueryable();

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

        public async Task<IEnumerable<TRoles>> GetRoleListAsync(Guid key)
        {
            var result = _context.TRoles.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<TClientUrls>> GetUriListAsync(Guid key)
        {
            var result = _context.TClientUrls.Where(x => x.ClientId == key).ToList();

            return await Task.FromResult(result);
        }

        public async Task<TClients> UpdateAsync(TClients model)
        {
            var entity = _context.TClients.Where(x => x.Id == model.Id).Single();

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
