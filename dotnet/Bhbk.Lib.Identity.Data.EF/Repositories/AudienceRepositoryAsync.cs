using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Primitives.Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.EF.Repositories
{
    public class AudienceRepositoryAsync : GenericRepositoryAsync<tbl_Audience>
    {
        private IClockService _clock;

        public AudienceRepositoryAsync(IdentityEntities context, IContextService env)
            : base(context)
        {
            _clock = new ClockService(env);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public async ValueTask<tbl_AudienceRole> AddRoleAsync(tbl_AudienceRole role, CancellationToken ct = default)
        {
            role.Created = Clock.UtcDateTime;
            await _context.Set<tbl_AudienceRole>().AddAsync(role, ct);
            return role;
        }

        public new async ValueTask<tbl_Audience> PostAsync(tbl_Audience audience, CancellationToken ct = default)
        {
            var entry = await _context.AddAsync(audience, ct);
            return entry.Entity;
        }

        public async ValueTask<tbl_Audience> PostAsync(tbl_Audience audience, string password, CancellationToken ct = default)
        {
            audience.ConcurrencyStamp = Guid.NewGuid().ToString();
            audience.SecurityStamp = Guid.NewGuid().ToString();

            var create = await PostAsync(audience, ct);
            await _context.SaveChangesAsync(ct);

            create = await SetPasswordAsync(create, password, ct);
            return create;
        }

        public new async ValueTask<tbl_Audience> DeleteAsync(tbl_Audience audience, CancellationToken ct = default)
        {
            var activityAudiences = _context.Set<tbl_AudienceActivity>().Where(x => x.AudienceId == audience.Id);
            var refreshes = _context.Set<tbl_Refresh>().Where(x => x.AudienceId == audience.Id);
            var settings = _context.Set<tbl_Setting>().Where(x => x.AudienceId == audience.Id);
            var states = _context.Set<tbl_State>().Where(x => x.AudienceId == audience.Id);
            var roles = _context.Set<tbl_Role>().Where(x => x.AudienceId == audience.Id);

            _context.RemoveRange(activityAudiences);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);
            _context.RemoveRange(roles);

            return _context.Remove(audience).Entity;
        }

        public async ValueTask<List<Claim>> GenerateAccessClaimsAsync(tbl_Issuer issuer, tbl_Audience audience, CancellationToken ct = default)
        {
            var expire = await _context.Set<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire)
                .SingleAsync(ct);

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, audience.Id.ToString()));

            var roles = await _context.Set<tbl_Role>()
                .Where(x => x.tbl_AudienceRoles.Any(y => y.AudienceId == audience.Id))
                .ToListAsync(ct);

            foreach (var role in roles.OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public async ValueTask<List<Claim>> GenerateRefreshClaimsAsync(tbl_Issuer issuer, tbl_Audience audience, CancellationToken ct = default)
        {
            var expire = await _context.Set<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.RefreshExpire)
                .SingleAsync(ct);

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, audience.Id.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public async ValueTask<bool> IsInRoleAsync(tbl_Audience audience, tbl_Role role, CancellationToken ct = default)
        {
            return await _context.Set<tbl_AudienceRole>()
                .AnyAsync(x => x.AudienceId == audience.Id && x.RoleId == role.Id, ct);
        }

        public async ValueTask<bool> IsPasswordSetAsync(tbl_Audience audience, CancellationToken ct = default)
        {
            var entity = await _context.Set<tbl_Audience>()
                .Where(x => x.Id == audience.Id)
                .SingleAsync(ct);

            return !string.IsNullOrEmpty(entity.PasswordHashPBKDF2);
        }

        public tbl_AudienceRole RemoveRole(tbl_AudienceRole role)
        {
            _context.Set<tbl_AudienceRole>().Remove(role);
            return role;
        }

        public async ValueTask<tbl_Audience> SetPasswordAsync(tbl_Audience audience, string password, CancellationToken ct = default)
        {
            var exists = await _context.Set<tbl_Audience>()
                .Where(x => x.Id == audience.Id && x.SecurityStamp == audience.SecurityStamp)
                .AnyAsync(ct);

            if (!exists)
                throw new InvalidOperationException();

            audience.ConcurrencyStamp = Guid.NewGuid().ToString();
            audience.SecurityStamp = Guid.NewGuid().ToString();

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

        public new async ValueTask PutAsync(tbl_Audience audience, CancellationToken ct = default)
        {
            var entity = await _context.Set<tbl_Audience>()
                .Where(x => x.Id == audience.Id)
                .SingleAsync(ct);

            if (entity.ConcurrencyStamp != audience.ConcurrencyStamp)
                throw new InvalidOperationException();

            entity.IssuerId = audience.IssuerId;
            entity.Name = audience.Name;
            entity.Description = audience.Description;
            entity.IsDeletable = audience.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;
            _context.Update(entity);
        }

        public async ValueTask<IEnumerable<tbl_Role>> GetRolesForAudienceAsync(Guid audienceId, CancellationToken ct = default)
        {
            var roleIds = await _context.Set<tbl_AudienceRole>()
                .Where(x => x.AudienceId == audienceId)
                .Select(x => x.RoleId)
                .ToListAsync(ct);

            return await _context.Set<tbl_Role>()
                .Where(x => roleIds.Contains(x.Id))
                .ToListAsync(ct);
        }
    }
}
