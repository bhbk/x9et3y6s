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

namespace Bhbk.Lib.Identity.Data.EF.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : GenericRepository<tbl_User>
    {
        private IClockService _clock;

        public UserRepository(IdentityEntities context, IContextService env)
            : base(context)
        {
            _clock = new ClockService(env);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public tbl_UserClaim AddClaim(tbl_UserClaim claim)
        {
            claim.Created = Clock.UtcDateTime;

            _context.Set<tbl_UserClaim>().Add(claim);

            return claim;
        }

        public tbl_UserLoginProvider AddLoginProvider(tbl_UserLoginProvider claim)
        {
            claim.Created = Clock.UtcDateTime;

            _context.Set<tbl_UserLoginProvider>().Add(claim);

            return claim;
        }

        public tbl_UserRole AddRole(tbl_UserRole role)
        {
            role.Created = Clock.UtcDateTime;

            _context.Set<tbl_UserRole>().Add(role);

            return role;
        }

        public new tbl_User Post(tbl_User user)
        {
            return _context.Add(user).Entity;
        }

        public tbl_User Post(tbl_User user, string password)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (!user.IsHumanBeing)
                user.EmailConfirmed = true;

            var create = Post(user);

            _context.SaveChanges();

            create = SetPassword(create, password);

            return create;
        }

        public new tbl_User Delete(tbl_User user)
        {
            var activity = _context.Set<tbl_UserAuthActivity>()
                .Where(x => x.UserId == user.Id);

            var entitlements = _context.Set<tbl_UserEntitlement>()
                .Where(x => x.UserId == user.Id);

            var refreshes = _context.Set<tbl_Refresh>()
                .Where(x => x.UserId == user.Id);

            var settings = _context.Set<tbl_Setting>()
                .Where(x => x.UserId == user.Id);

            var states = _context.Set<tbl_State>()
                .Where(x => x.UserId == user.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(entitlements);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);

            return _context.Remove(user).Entity;
        }

        [Obsolete]
        public List<Claim> GenerateAccessClaims(tbl_User user)
        {
            var legacyClaims = _context.Set<tbl_Setting>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (!string.IsNullOrEmpty(user.EmailAddress))
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            var userRoles = _context.Set<tbl_Role>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == user.Id)).ToList();

            foreach (var role in userRoles.OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                /* legacy claim format for backward compatibility */
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = _context.Set<tbl_Claim>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == user.Id)).ToList();

            foreach (var claim in userClaims.OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(86400).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public List<Claim> GenerateAccessClaims(tbl_Issuer issuer, tbl_User user)
        {
            var expire = _context.Set<tbl_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.AccessExpire).Single();

            var legacyClaims = _context.Set<tbl_Setting>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (!string.IsNullOrEmpty(user.EmailAddress))
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            var userRoles = _context.Set<tbl_Role>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == user.Id)).ToList();

            foreach (var role in userRoles.OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                /* legacy claim format for backward compatibility */
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = _context.Set<tbl_Claim>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == user.Id)).ToList();

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

        public List<Claim> GenerateRefreshClaims(tbl_Issuer issuer, tbl_User user)
        {
            var expire = _context.Set<tbl_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.RefreshExpire).Single();

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

        public bool IsInClaim(tbl_User user, tbl_Claim claim)
        {
            /*
             * TODO need to add check for role based claims...
             */

            if (_context.Set<tbl_UserClaim>()
                .Any(x => x.UserId == user.Id && x.ClaimId == claim.Id))
                return true;

            return false;
        }

        public bool IsInLoginProvider(tbl_User user, tbl_LoginProvider loginProvider)
        {
            if (_context.Set<tbl_UserLoginProvider>()
                .Any(x => x.UserId == user.Id && x.LoginProviderId == loginProvider.Id))
                return true;

            return false;
        }

        public bool IsInRole(tbl_User user, tbl_Role role)
        {
            if (_context.Set<tbl_UserRole>()
                .Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                return true;

            return false;
        }

        public bool IsLockedOut(tbl_User entity)
        {
            if (entity.IsLockedOut)
            {
                if (entity.LockoutEnd.HasValue && entity.LockoutEnd <= DateTime.UtcNow)
                {
                    entity.IsLockedOut = false;
                    entity.LockoutEnd = null;

                    Put(entity);

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                Put(entity);

                return false;
            }
        }

        public bool IsPasswordSet(tbl_User user)
        {
            var entity = _context.Set<tbl_User>()
                .Where(x => x.Id == user.Id).Single();

            if (string.IsNullOrEmpty(entity.PasswordHashPBKDF2))
                return false;

            return true;
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

        public tbl_User SetPassword(tbl_User user, string password)
        {
            /* https://www.google.com/search?q=identity+securitystamp */
            if (!_context.Set<tbl_User>()
                .Where(x => x.Id == user.Id && x.SecurityStamp == user.SecurityStamp)
                .Any())
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

        public new void Put(tbl_User user)
        {
            var entity = _context.Set<tbl_User>()
                .Where(x => x.Id == user.Id).Single();

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

        public IEnumerable<tbl_Role> GetRolesForUser(Guid userId)
        {
            var roleIds = _context.Set<tbl_UserRole>()
                .Where(x => x.UserId == userId)
                .Select(x => x.RoleId).ToList();
            return _context.Set<tbl_Role>().Where(x => roleIds.Contains(x.Id)).ToList();
        }
    }
}