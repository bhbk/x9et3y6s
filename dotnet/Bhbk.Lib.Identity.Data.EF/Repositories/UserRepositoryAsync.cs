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
    public class UserRepositoryAsync : GenericRepositoryAsync<tbl_User>
    {
        private IClockService _clock;

        public UserRepositoryAsync(IdentityEntities context, IContextService env)
            : base(context)
        {
            _clock = new ClockService(env);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public async ValueTask<tbl_UserClaim> AddClaimAsync(tbl_UserClaim claim, CancellationToken ct = default)
        {
            claim.Created = Clock.UtcDateTime;
            await _context.Set<tbl_UserClaim>().AddAsync(claim, ct);
            return claim;
        }

        public async ValueTask<tbl_UserLoginProvider> AddLoginProviderAsync(tbl_UserLoginProvider login, CancellationToken ct = default)
        {
            login.Created = Clock.UtcDateTime;
            await _context.Set<tbl_UserLoginProvider>().AddAsync(login, ct);
            return login;
        }

        public async ValueTask<tbl_UserRole> AddRoleAsync(tbl_UserRole role, CancellationToken ct = default)
        {
            role.Created = Clock.UtcDateTime;
            await _context.Set<tbl_UserRole>().AddAsync(role, ct);
            return role;
        }

        public new async ValueTask<tbl_User> PostAsync(tbl_User user, CancellationToken ct = default)
        {
            var entry = await _context.AddAsync(user, ct);
            return entry.Entity;
        }

        public async ValueTask<tbl_User> PostAsync(tbl_User user, string password, CancellationToken ct = default)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (!user.IsHumanBeing)
                user.EmailConfirmed = true;

            var create = await PostAsync(user, ct);
            await _context.SaveChangesAsync(ct);

            create = await SetPasswordAsync(create, password, ct);
            return create;
        }

        public new async ValueTask<tbl_User> DeleteAsync(tbl_User user, CancellationToken ct = default)
        {
            var activity = _context.Set<tbl_UserActivity>().Where(x => x.UserId == user.Id);
            var refreshes = _context.Set<tbl_Refresh>().Where(x => x.UserId == user.Id);
            var settings = _context.Set<tbl_Setting>().Where(x => x.UserId == user.Id);
            var states = _context.Set<tbl_State>().Where(x => x.UserId == user.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);

            return _context.Remove(user).Entity;
        }

        public async ValueTask<List<Claim>> GenerateAccessClaimsAsync(tbl_Issuer issuer, tbl_User user, CancellationToken ct = default)
        {
            var expire = await _context.Set<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire)
                .SingleAsync(ct);

            var legacyClaims = await _context.Set<tbl_Setting>()
                .Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.GlobalLegacyClaims)
                .SingleAsync(ct);

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (!string.IsNullOrEmpty(user.EmailAddress))
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            var userRoles = await _context.Set<tbl_Role>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == user.Id))
                .ToListAsync(ct);

            foreach (var role in userRoles.OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = await _context.Set<tbl_Claim>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == user.Id))
                .ToListAsync(ct);

            foreach (var claim in userClaims.OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public async ValueTask<List<Claim>> GenerateRefreshClaimsAsync(tbl_Issuer issuer, tbl_User user, CancellationToken ct = default)
        {
            var expire = await _context.Set<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.RefreshExpire)
                .SingleAsync(ct);

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public async ValueTask<bool> IsInClaimAsync(tbl_User user, tbl_Claim claim, CancellationToken ct = default)
        {
            return await _context.Set<tbl_UserClaim>()
                .AnyAsync(x => x.UserId == user.Id && x.ClaimId == claim.Id, ct);
        }

        public async ValueTask<bool> IsInLoginProviderAsync(tbl_User user, tbl_LoginProvider loginProvider, CancellationToken ct = default)
        {
            return await _context.Set<tbl_UserLoginProvider>()
                .AnyAsync(x => x.UserId == user.Id && x.LoginProviderId == loginProvider.Id, ct);
        }

        public async ValueTask<bool> IsInRoleAsync(tbl_User user, tbl_Role role, CancellationToken ct = default)
        {
            return await _context.Set<tbl_UserRole>()
                .AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id, ct);
        }

        public async ValueTask<bool> IsLockedOutAsync(tbl_User entity, CancellationToken ct = default)
        {
            if (entity.IsLockedOut)
            {
                if (entity.LockoutEnd.HasValue && entity.LockoutEnd <= DateTime.UtcNow)
                {
                    entity.IsLockedOut = false;
                    entity.LockoutEnd = null;
                    await PutAsync(entity, ct);
                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                await PutAsync(entity, ct);
                return false;
            }
        }

        public async ValueTask<bool> IsPasswordSetAsync(tbl_User user, CancellationToken ct = default)
        {
            var entity = await _context.Set<tbl_User>()
                .Where(x => x.Id == user.Id)
                .SingleAsync(ct);

            return !string.IsNullOrEmpty(entity.PasswordHashPBKDF2);
        }

        public tbl_UserClaim RemoveClaim(tbl_UserClaim claim)
        {
            _context.Set<tbl_UserClaim>().Remove(claim);
            return claim;
        }

        public tbl_UserLoginProvider RemoveLoginProvider(tbl_UserLoginProvider login)
        {
            _context.Set<tbl_UserLoginProvider>().Remove(login);
            return login;
        }

        public tbl_UserRole RemoveRole(tbl_UserRole role)
        {
            _context.Set<tbl_UserRole>().Remove(role);
            return role;
        }

        public tbl_User SetConfirmedEmail(tbl_User user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            _context.Entry(user).State = EntityState.Modified;
            return _context.Entry(user).Entity;
        }

        public tbl_User SetConfirmedPassword(tbl_User user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            _context.Entry(user).State = EntityState.Modified;
            return _context.Entry(user).Entity;
        }

        public tbl_User SetConfirmedPhoneNumber(tbl_User user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            _context.Entry(user).State = EntityState.Modified;
            return _context.Entry(user).Entity;
        }

        public tbl_User SetImmutable(tbl_User user, bool enabled)
        {
            user.IsDeletable = enabled;
            _context.Entry(user).State = EntityState.Modified;
            return _context.Entry(user).Entity;
        }

        public async ValueTask<tbl_User> SetPasswordAsync(tbl_User user, string password, CancellationToken ct = default)
        {
            var exists = await _context.Set<tbl_User>()
                .Where(x => x.Id == user.Id && x.SecurityStamp == user.SecurityStamp)
                .AnyAsync(ct);

            if (!exists)
                throw new InvalidOperationException();

            if (string.IsNullOrEmpty(password))
            {
                user.PasswordHashPBKDF2 = null;
                user.PasswordHashSHA256 = null;
            }
            else
            {
                user.PasswordHashPBKDF2 = PBKDF2.Create(password);
                user.PasswordHashSHA256 = SHA256.Create(password);
            }

            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            _context.Entry(user).State = EntityState.Modified;
            return _context.Entry(user).Entity;
        }

        public new async ValueTask PutAsync(tbl_User user, CancellationToken ct = default)
        {
            var entity = await _context.Set<tbl_User>()
                .Where(x => x.Id == user.Id)
                .SingleAsync(ct);

            if (entity.ConcurrencyStamp != user.ConcurrencyStamp)
                throw new InvalidOperationException();

            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            entity.IsLockedOut = user.IsLockedOut;
            entity.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
            entity.IsDeletable = user.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;
            _context.Update(entity);
        }

        public async ValueTask<IEnumerable<tbl_Role>> GetRolesForUserAsync(Guid userId, CancellationToken ct = default)
        {
            var roleIds = await _context.Set<tbl_UserRole>()
                .Where(x => x.UserId == userId)
                .Select(x => x.RoleId)
                .ToListAsync(ct);

            return await _context.Set<tbl_Role>()
                .Where(x => roleIds.Contains(x.Id))
                .ToListAsync(ct);
        }
    }
}
