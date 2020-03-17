using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class AudienceRepository : GenericRepository<tbl_Audiences>
    {
        private IClockService _clock;

        public AudienceRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public tbl_Audiences AccessFailed(tbl_Audiences audience)
        {
            audience.LastLoginFailure = Clock.UtcDateTime;
            audience.AccessFailedCount++;

            _context.Entry(audience).State = EntityState.Modified;

            return _context.Entry(audience).Entity;
        }

        public tbl_Audiences AccessSuccess(tbl_Audiences audience)
        {
            audience.LastLoginSuccess = Clock.UtcDateTime;
            audience.AccessSuccessCount++;

            _context.Entry(audience).State = EntityState.Modified;

            return _context.Entry(audience).Entity;
        }

        public bool AddToRole(tbl_Audiences audience, tbl_Roles role)
        {
            _context.Set<tbl_AudienceRoles>().Add(
                new tbl_AudienceRoles()
                {
                    AudienceId = audience.Id,
                    RoleId = role.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return true;
        }

        public override tbl_Audiences Create(tbl_Audiences audience)
        {
            var create = InternalCreate(audience);

            _context.SaveChanges();

            return create;
        }

        public tbl_Audiences Create(tbl_Audiences audience, string password)
        {
            var create = InternalCreate(audience);

            _context.SaveChanges();

            SetPasswordHash(audience, password);

            return create;
        }

        public override tbl_Audiences Delete(tbl_Audiences audience)
        {
            var activity = _context.Set<tbl_Activities>()
                .Where(x => x.AudienceId == audience.Id);

            var refreshes = _context.Set<tbl_Refreshes>()
                .Where(x => x.AudienceId == audience.Id);

            var settings = _context.Set<tbl_Settings>()
                .Where(x => x.AudienceId == audience.Id);

            var states = _context.Set<tbl_States>()
                .Where(x => x.AudienceId == audience.Id);

            var roles = _context.Set<tbl_Roles>()
                .Where(x => x.AudienceId == audience.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);
            _context.RemoveRange(roles);

            return _context.Remove(audience).Entity;
        }

        public List<Claim> GenerateAccessClaims(tbl_Issuers issuer, tbl_Audiences audience)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingAccessExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, audience.Id.ToString()));

            var roles = _context.Set<tbl_Roles>()
                .Where(x => x.tbl_AudienceRoles.Any(y => y.AudienceId == audience.Id)).ToList();

            foreach (var role in roles.OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, 
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, 
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public List<Claim> GenerateRefreshClaims(tbl_Issuers issuer, tbl_Audiences audience)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, audience.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, 
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, 
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        internal tbl_Audiences InternalCreate(tbl_Audiences audience)
        {
            audience.ConcurrencyStamp = Guid.NewGuid().ToString();
            audience.SecurityStamp = Guid.NewGuid().ToString();

            return _context.Add(audience).Entity;
        }

        public bool IsInRole(tbl_Audiences audience, tbl_Roles role)
        {
            if (_context.Set<tbl_AudienceRoles>()
                .Any(x => x.AudienceId == audience.Id && x.RoleId == role.Id))
                return true;

            return false;
        }

        public bool RemoveFromRole(tbl_Audiences audience, tbl_Roles role)
        {
            var entity = _context.Set<tbl_AudienceRoles>()
                .Where(x => x.AudienceId == audience.Id && x.RoleId == role.Id).Single();

            _context.Set<tbl_AudienceRoles>().Remove(entity);

            return true;
        }

        public tbl_Audiences SetPasswordHash(tbl_Audiences audience, string password)
        {
            //https://www.google.com/search?q=identity+securitystamp
            if (!_context.Set<tbl_Audiences>()
                .Where(x => x.Id == audience.Id && x.SecurityStamp == audience.SecurityStamp)
                .Any())
                throw new InvalidOperationException();

            audience.ConcurrencyStamp = Guid.NewGuid().ToString();
            audience.SecurityStamp = Guid.NewGuid().ToString();
            audience.LastUpdated = Clock.UtcDateTime;

            if (!string.IsNullOrEmpty(password))
            {
                audience.PasswordHashPBKDF2 = PBKDF2.Create(password);
                audience.PasswordHashSHA256 = SHA256.Create(password);
            }

            _context.Entry(audience).State = EntityState.Modified;

            return _context.Entry(audience).Entity;
        }

        public override tbl_Audiences Update(tbl_Audiences audience)
        {
            var entity = _context.Set<tbl_Audiences>()
                .Where(x => x.Id == audience.Id).Single();

            //https://www.google.com/search?q=identity+concurrencystamp
            if (entity.ConcurrencyStamp != audience.ConcurrencyStamp)
                throw new InvalidOperationException();

            /*
             * only persist certain fields.
             */
            entity.IssuerId = audience.IssuerId;
            entity.Name = audience.Name;
            entity.Description = audience.Description;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Enabled = audience.Enabled;
            entity.Immutable = audience.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
