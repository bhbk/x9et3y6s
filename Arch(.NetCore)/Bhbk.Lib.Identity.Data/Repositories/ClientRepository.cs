using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class ClientRepository : GenericRepository<tbl_Clients>
    {
        private IClockService _clock;

        public ClientRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance)
        {
            _clock = new ClockService(new ContextService(instance));
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public tbl_Clients AccessFailed(tbl_Clients client)
        {
            client.LastLoginFailure = Clock.UtcDateTime;
            client.AccessFailedCount++;

            _context.Entry(client).State = EntityState.Modified;

            return _context.Entry(client).Entity;
        }

        public tbl_Clients AccessSuccess(tbl_Clients client)
        {
            client.LastLoginSuccess = Clock.UtcDateTime;
            client.AccessSuccessCount++;

            _context.Entry(client).State = EntityState.Modified;

            return _context.Entry(client).Entity;
        }

        public override tbl_Clients Delete(tbl_Clients client)
        {
            var activity = _context.Set<tbl_Activities>().Where(x => x.ClientId == client.Id);
            var refreshes = _context.Set<tbl_Refreshes>().Where(x => x.ClientId == client.Id);
            var settings = _context.Set<tbl_Settings>().Where(x => x.ClientId == client.Id);
            var states = _context.Set<tbl_States>().Where(x => x.ClientId == client.Id);
            var roles = _context.Set<tbl_Roles>().Where(x => x.ClientId == client.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);
            _context.RemoveRange(roles);
            _context.Remove(client);

            return _context.Remove(client).Entity;
        }

        public List<Claim> GenerateAccessClaims(tbl_Issuers issuer, tbl_Clients client)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingAccessExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

            //service identity vs. a user identity
            claims.Add(new Claim(ClaimTypes.System, ClientType.server.ToString()));

            var roles = _context.Set<tbl_Roles>()
                .Where(x => x.tbl_ClientRoles.Any(y => y.ClientId == client.Id)).ToList();

            foreach (var role in roles.OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public List<Claim> GenerateRefreshClaims(tbl_Issuers issuer, tbl_Clients client)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public override tbl_Clients Update(tbl_Clients client)
        {
            var entity = _context.Set<tbl_Clients>()
                .Where(x => x.Id == client.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.IssuerId = client.IssuerId;
            entity.Name = client.Name;
            entity.Description = client.Description;
            entity.ClientType = client.ClientType;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Enabled = client.Enabled;
            entity.Immutable = client.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
