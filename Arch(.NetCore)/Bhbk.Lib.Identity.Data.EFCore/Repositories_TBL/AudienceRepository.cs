using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_TBL;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_TBL
{
    public class AudienceRepository : GenericRepository<tbl_Audience>
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

        public tbl_Audience AccessFailed(tbl_Audience audience)
        {
            audience.LastLoginFailureUtc = Clock.UtcDateTime;
            audience.AccessFailedCount++;

            _context.Entry(audience).State = EntityState.Modified;

            return _context.Entry(audience).Entity;
        }

        public tbl_Audience AccessSuccess(tbl_Audience audience)
        {
            audience.LastLoginSuccessUtc = Clock.UtcDateTime;
            audience.AccessSuccessCount++;

            _context.Entry(audience).State = EntityState.Modified;

            return _context.Entry(audience).Entity;
        }

        public tbl_AudienceRole AddRole(tbl_AudienceRole role)
        {
            role.CreatedUtc = Clock.UtcDateTime;

            _context.Set<tbl_AudienceRole>().Add(role);

            return role;
        }

        public override tbl_Audience Create(tbl_Audience audience)
        {
            return _context.Add(audience).Entity;
        }

        public tbl_Audience Create(tbl_Audience audience, string password)
        {
            audience.ConcurrencyStamp = Guid.NewGuid().ToString();
            audience.SecurityStamp = Guid.NewGuid().ToString();

            var create = Create(audience);

            _context.SaveChanges();

            create = SetPassword(create, password);

            return create;
        }

        public override tbl_Audience Delete(tbl_Audience audience)
        {
            var activity = _context.Set<tbl_Activity>()
                .Where(x => x.AudienceId == audience.Id);

            var refreshes = _context.Set<tbl_Refresh>()
                .Where(x => x.AudienceId == audience.Id);

            var settings = _context.Set<tbl_Setting>()
                .Where(x => x.AudienceId == audience.Id);

            var states = _context.Set<tbl_State>()
                .Where(x => x.AudienceId == audience.Id);

            var roles = _context.Set<tbl_Role>()
                .Where(x => x.AudienceId == audience.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);
            _context.RemoveRange(roles);

            return _context.Remove(audience).Entity;
        }

        public List<Claim> GenerateAccessClaims(tbl_Issuer issuer, tbl_Audience audience)
        {
            var expire = _context.Set<tbl_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingAccessExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, audience.Id.ToString()));

            var roles = _context.Set<tbl_Role>()
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

        public List<Claim> GenerateRefreshClaims(tbl_Issuer issuer, tbl_Audience audience)
        {
            var expire = _context.Set<tbl_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
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

        public bool IsInRole(tbl_Audience audience, tbl_Role role)
        {
            if (_context.Set<tbl_AudienceRole>()
                .Any(x => x.AudienceId == audience.Id && x.RoleId == role.Id))
                return true;

            return false;
        }

        public bool IsPasswordSet(tbl_Audience audience)
        {
            var entity = _context.Set<tbl_Audience>()
                .Where(x => x.Id == audience.Id).Single();

            if (string.IsNullOrEmpty(entity.PasswordHashPBKDF2))
                return false;

            return true;
        }

        public tbl_AudienceRole RemoveRole(tbl_AudienceRole role)
        {
            _context.Set<tbl_AudienceRole>().Remove(role);

            return role;
        }

        public tbl_Audience SetPassword(tbl_Audience audience, string password)
        {
            //https://www.google.com/search?q=identity+securitystamp
            if (!_context.Set<tbl_Audience>()
                .Where(x => x.Id == audience.Id && x.SecurityStamp == audience.SecurityStamp)
                .Any())
                throw new InvalidOperationException();

            audience.ConcurrencyStamp = Guid.NewGuid().ToString();
            audience.SecurityStamp = Guid.NewGuid().ToString();
            audience.LastUpdatedUtc = Clock.UtcDateTime;

            if (string.IsNullOrEmpty(password))
            {
                audience.PasswordHashPBKDF2 = null;
                audience.PasswordHashSHA256 = null;
            }
            else
            {
                audience.PasswordHashPBKDF2 = PBKDF2.Create(password);
                audience.PasswordHashSHA256 = SHA256.Create(password);
            }

            _context.Entry(audience).State = EntityState.Modified;

            return _context.Entry(audience).Entity;
        }

        public override tbl_Audience Update(tbl_Audience audience)
        {
            var entity = _context.Set<tbl_Audience>()
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
            entity.LastUpdatedUtc = Clock.UtcDateTime;
            entity.IsDeletable = audience.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
