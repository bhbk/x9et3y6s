using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
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
        private readonly ContextType _situation;
        private readonly IConfigurationRoot _conf;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context, ContextType situation, IConfigurationRoot conf, IMapper mapper)
        {
            if (context == null)
                throw new NullReferenceException();

            _context = context;
            _situation = situation;
            _conf = conf;
            _mapper = mapper;
        }

        public async Task<AppClientUri> AddUriAsync(ClientUriCreate model)
        {
            var entity = _mapper.Map<AppClientUri>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<AppClientUri>(result));
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
            var entity = _mapper.Map<AppClient>(model);
            var result = _context.Add(entity).Entity;

            return await Task.FromResult(_mapper.Map<AppClient>(result));
        }

        public async Task<ClaimsPrincipal> CreateAccessAsync(AppClient client)
        {
            /*
             * moving away from microsoft constructs for identity implementation because of un-needed additional 
             * layers of complexity, and limitations, for the simple operations needing to be performed.
             * 
             * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iuserclaimsprincipalfactory-1
             */

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

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

        public async Task<ClaimsPrincipal> CreateRefreshAsync(AppClient client)
        {
            /*
             * moving away from microsoft constructs for identity implementation because of un-needed additional 
             * layers of complexity, and limitations, for the simple operations needing to be performed.
             * 
             * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iuserclaimsprincipalfactory-1
             */

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

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppClient.Any(x => x.Id == key));
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

            return await Task.FromResult(_mapper.Map<AppClient>(_context.Update(entity).Entity));
        }
    }
}
