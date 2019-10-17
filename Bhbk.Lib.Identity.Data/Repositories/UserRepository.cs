using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Data.Validators;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : GenericRepositoryAsync<tbl_Users>
    {
        private IClockService _clock;
        public readonly PasswordValidator passwordValidator;
        public readonly PasswordHasher passwordHasher;
        public readonly UserValidator userValidator;

        public UserRepository(_DbContext context, InstanceContext instance)
            : base(context, instance)
        {
            _clock = new ClockService(_instance);

            passwordValidator = new PasswordValidator();
            passwordHasher = new PasswordHasher();
            userValidator = new UserValidator();
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public async Task<tbl_Users> AccessFailedAsync(tbl_Users user)
        {
            user.LastLoginFailure = Clock.UtcDateTime;
            user.AccessFailedCount++;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public async Task<tbl_Users> AccessSuccessAsync(tbl_Users user)
        {
            user.LastLoginSuccess = Clock.UtcDateTime;
            user.AccessSuccessCount++;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public async Task<bool> AddToClaimAsync(tbl_Users user, tbl_Claims claim)
        {
            _context.Set<tbl_UserClaims>().Add(
                new tbl_UserClaims()
                {
                    UserId = user.Id,
                    ClaimId = claim.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> AddToLoginAsync(tbl_Users user, tbl_Logins login)
        {
            _context.Set<tbl_UserLogins>().Add(
                new tbl_UserLogins()
                {
                    UserId = user.Id,
                    LoginId = login.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> AddToRoleAsync(tbl_Users user, tbl_Roles role)
        {
            _context.Set<tbl_UserRoles>().Add(
                new tbl_UserRoles()
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public override async Task<tbl_Users> CreateAsync(tbl_Users user)
        {
            var create = await InternalCreateAsync(user);

            _context.SaveChanges();

            return await Task.FromResult(create);
        }

        public async Task<tbl_Users> CreateAsync(tbl_Users user, string password)
        {
            var create = await InternalCreateAsync(user);

            _context.SaveChanges();

            await InternalSetPasswordAsync(user, password);

            return await Task.FromResult(create);
        }

        public override async Task<tbl_Users> DeleteAsync(tbl_Users user)
        {
            var activity = _context.Set<tbl_Activities>().Where(x => x.UserId == user.Id);
            var refreshes = _context.Set<tbl_Refreshes>().Where(x => x.UserId == user.Id);
            var settings = _context.Set<tbl_Settings>().Where(x => x.UserId == user.Id);
            var states = _context.Set<tbl_States>().Where(x => x.UserId == user.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);

            return await Task.FromResult(_context.Remove(user).Entity);
        }

        public async Task<ClaimsPrincipal> GenerateAccessClaimsAsync(tbl_Issuers issuer, tbl_Users user)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingAccessExpire).Single();

            var legacyClaims = _context.Set<tbl_Settings>().Where(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            //defaults...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            //user identity vs. a service identity
            claims.Add(new Claim(ClaimTypes.System, ClientType.user_agent.ToString()));

            var userRoles = _context.Set<tbl_Roles>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == user.Id)).ToList();

            foreach (var role in userRoles.OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                //check compatibility is enabled. pack claim(s) with old name and new name.
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = _context.Set<tbl_Claims>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == user.Id)).ToList();

            foreach (var claim in userClaims.OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, Base64.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<ClaimsPrincipal> GenerateRefreshClaimsAsync(tbl_Issuers issuer, tbl_Users user)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //defaults...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, Base64.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        internal async Task<tbl_Users> InternalCreateAsync(tbl_Users user)
        {
            if (!(await userValidator.ValidateAsync(user)).Succeeded)
                throw new InvalidOperationException();

            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            return await Task.FromResult(_context.Add(user).Entity);
        }

        internal async Task<bool> InternalSetPasswordHashAsync(tbl_Users user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> InternalSetSecurityStampAsync(tbl_Users user, string stamp)
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> InternalSetPasswordAsync(tbl_Users user, string password)
        {
            if (passwordValidator == null)
                throw new NotSupportedException();

            var result = await passwordValidator.ValidateAsync(user, password);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            if (passwordHasher == null)
                throw new NotSupportedException();

            var hash = passwordHasher.HashPassword(user, password);

            if (!await InternalSetPasswordHashAsync(user, hash)
                || !await InternalSetSecurityStampAsync(user, Base64.CreateString(32)))
                return false;

            return true;
        }

        internal async Task<PasswordVerificationResult> InternalVerifyPasswordAsync(tbl_Users user, string password)
        {
            if (passwordHasher == null)
                throw new NotSupportedException();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return await Task.FromResult(PasswordVerificationResult.Failed);
        }

        public async Task<bool> IsInClaimAsync(Guid userKey, Guid claimKey)
        {
            /*
             * TODO need to add check for role based claims...
             */

            if (_context.Set<tbl_UserClaims>().Any(x => x.UserId == userKey && x.ClaimId == claimKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInLoginAsync(Guid userKey, Guid loginKey)
        {
            if (_context.Set<tbl_UserLogins>().Any(x => x.UserId == userKey && x.LoginId == loginKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInRoleAsync(Guid userKey, Guid roleKey)
        {
            if (_context.Set<tbl_UserRoles>().Any(x => x.UserId == userKey && x.RoleId == roleKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsLockedOutAsync(Guid key)
        {
            var entity = _context.Set<tbl_Users>().Where(x => x.Id == key).SingleOrDefault();

            if (entity.LockoutEnabled)
            {
                if (entity.LockoutEnd.HasValue && entity.LockoutEnd <= DateTime.UtcNow)
                {
                    entity.LockoutEnabled = false;
                    entity.LockoutEnd = null;

                    await UpdateAsync(entity);

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                await UpdateAsync(entity);

                return false;
            }
        }

        public async Task<bool> IsPasswordSetAsync(Guid key)
        {
            var entity = _context.Set<tbl_Users>().Where(x => x.Id == key).SingleOrDefault();

            if (entity == null)
                return await Task.FromResult(false);

            else if (string.IsNullOrEmpty(entity.PasswordHash))
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromClaimAsync(tbl_Users user, tbl_Claims claim)
        {
            var entity = _context.Set<tbl_UserClaims>().Where(x => x.UserId == user.Id && x.ClaimId == claim.Id).Single();

            _context.Set<tbl_UserClaims>().Remove(entity);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromLoginAsync(tbl_Users user, tbl_Logins login)
        {
            var entity = _context.Set<tbl_UserLogins>().Where(x => x.UserId == user.Id && x.LoginId == login.Id).Single();

            _context.Set<tbl_UserLogins>().Remove(entity);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromRoleAsync(tbl_Users user, tbl_Roles role)
        {
            var entity = _context.Set<tbl_UserRoles>().Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.Set<tbl_UserRoles>().Remove(entity);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemovePasswordAsync(tbl_Users user)
        {
            return await InternalSetPasswordHashAsync(user, null);
        }

        public async Task<tbl_Users> SetConfirmedEmailAsync(tbl_Users user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public async Task<tbl_Users> SetConfirmedPasswordAsync(tbl_Users user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public async Task<tbl_Users> SetConfirmedPhoneNumberAsync(tbl_Users user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public async Task<tbl_Users> SetImmutableAsync(tbl_Users user, bool enabled)
        {
            user.Immutable = enabled;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public async Task<tbl_Users> SetPasswordAsync(tbl_Users user, string password)
        {
            await InternalSetPasswordAsync(user, password);

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public async Task<tbl_Users> SetTwoFactorEnabledAsync(tbl_Users user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Entry(user).Entity);
        }

        public override async Task<tbl_Users> UpdateAsync(tbl_Users user)
        {
            if (!(await userValidator.ValidateAsync(user)).Succeeded)
                throw new InvalidOperationException();

            var entity = _context.Set<tbl_Users>().Where(x => x.Id == user.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            entity.LockoutEnabled = user.LockoutEnabled;
            entity.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Immutable = user.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }

        public async Task<bool> VerifyPasswordAsync(Guid key, string password)
        {
            var entity = _context.Set<tbl_Users>().Where(x => x.Id == key).Single();

            if (await InternalVerifyPasswordAsync(entity, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }
    }
}