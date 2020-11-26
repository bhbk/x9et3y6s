using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models_TBL;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Data.Repositories_TBL
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

        public UserRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public tbl_User AccessFailed(tbl_User user)
        {
            user.LastLoginFailureUtc = Clock.UtcDateTime;
            user.AccessFailedCount++;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_User AccessSuccess(tbl_User user)
        {
            user.LastLoginSuccessUtc = Clock.UtcDateTime;
            user.AccessSuccessCount++;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_UserClaim AddClaim(tbl_UserClaim claim)
        {
            claim.CreatedUtc = Clock.UtcDateTime;

            _context.Set<tbl_UserClaim>().Add(claim);

            return claim;
        }

        public tbl_UserLogin AddLogin(tbl_UserLogin claim)
        {
            claim.CreatedUtc = Clock.UtcDateTime;

            _context.Set<tbl_UserLogin>().Add(claim);

            return claim;
        }

        public tbl_UserRole AddRole(tbl_UserRole role)
        {
            role.CreatedUtc = Clock.UtcDateTime;

            _context.Set<tbl_UserRole>().Add(role);

            return role;
        }

        public override tbl_User Create(tbl_User user)
        {
            return _context.Add(user).Entity;
        }

        public tbl_User Create(tbl_User user, string password)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (!user.IsHumanBeing)
                user.EmailConfirmed = true;

            var create = Create(user);

            _context.SaveChanges();

            create = SetPassword(create, password);

            return create;
        }

        public override tbl_User Delete(tbl_User user)
        {
            var activity = _context.Set<tbl_Activity>()
                .Where(x => x.UserId == user.Id);

            var refreshes = _context.Set<tbl_Refresh>()
                .Where(x => x.UserId == user.Id);

            var settings = _context.Set<tbl_Setting>()
                .Where(x => x.UserId == user.Id);

            var states = _context.Set<tbl_State>()
                .Where(x => x.UserId == user.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);

            return _context.Remove(user).Entity;
        }

        [Obsolete]
        public List<Claim> GenerateAccessClaims(tbl_User user)
        {
            var legacyClaims = _context.Set<tbl_Setting>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
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

                //check compatibility is enabled. pack claim(s) with old name and new name.
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = _context.Set<tbl_Claim>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == user.Id)).ToList();

            foreach (var claim in userClaims.OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

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
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(86400).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public List<Claim> GenerateAccessClaims(tbl_Issuer issuer, tbl_User user)
        {
            var expire = _context.Set<tbl_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingAccessExpire).Single();

            var legacyClaims = _context.Set<tbl_Setting>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
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

                //check compatibility is enabled. pack claim(s) with old name and new name.
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = _context.Set<tbl_Claim>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == user.Id)).ToList();

            foreach (var claim in userClaims.OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

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

        public List<Claim> GenerateRefreshClaims(tbl_Issuer issuer, tbl_User user)
        {
            var expire = _context.Set<tbl_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

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

        public bool IsInLogin(tbl_User user, tbl_Login login)
        {
            if (_context.Set<tbl_UserLogin>()
                .Any(x => x.UserId == user.Id && x.LoginId == login.Id))
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
                if (entity.LockoutEndUtc.HasValue && entity.LockoutEndUtc <= DateTime.UtcNow)
                {
                    entity.IsLockedOut = false;
                    entity.LockoutEndUtc = null;

                    Update(entity);

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEndUtc = null;
                Update(entity);

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

        public tbl_UserLogin RemoveLogin(tbl_UserLogin login)
        {
            _context.Set<tbl_UserLogin>().Remove(login);

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
            user.LastUpdatedUtc = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_User SetConfirmedPassword(tbl_User user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdatedUtc = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_User SetConfirmedPhoneNumber(tbl_User user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdatedUtc = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_User SetImmutable(tbl_User user, bool enabled)
        {
            user.IsDeletable = enabled;
            user.LastUpdatedUtc = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_User SetPassword(tbl_User user, string password)
        {
            //https://www.google.com/search?q=identity+securitystamp
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
            user.LastUpdatedUtc = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public override tbl_User Update(tbl_User user)
        {
            var entity = _context.Set<tbl_User>()
                .Where(x => x.Id == user.Id).Single();

            //https://www.google.com/search?q=identity+concurrencystamp
            if (entity.ConcurrencyStamp != user.ConcurrencyStamp)
                throw new InvalidOperationException();

            /*
             * only persist certain fields.
             */
            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            entity.IsLockedOut = user.IsLockedOut;
            entity.LockoutEndUtc = user.LockoutEndUtc.HasValue ? user.LockoutEndUtc.Value.ToUniversalTime() : user.LockoutEndUtc;
            entity.LastUpdatedUtc = Clock.UtcDateTime;
            entity.IsDeletable = user.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}