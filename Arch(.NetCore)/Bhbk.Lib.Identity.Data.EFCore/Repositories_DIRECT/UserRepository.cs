﻿using Bhbk.Lib.Common.Services;
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
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : GenericRepository<tbl_Users>
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

        public tbl_Users AccessFailed(tbl_Users user)
        {
            user.LastLoginFailure = Clock.UtcDateTime;
            user.AccessFailedCount++;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_Users AccessSuccess(tbl_Users user)
        {
            user.LastLoginSuccess = Clock.UtcDateTime;
            user.AccessSuccessCount++;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public bool AddToClaim(tbl_Users user, tbl_Claims claim)
        {
            _context.Set<tbl_UserClaims>().Add(
                new tbl_UserClaims()
                {
                    UserId = user.Id,
                    ClaimId = claim.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return true;
        }

        public bool AddToLogin(tbl_Users user, tbl_Logins login)
        {
            _context.Set<tbl_UserLogins>().Add(
                new tbl_UserLogins()
                {
                    UserId = user.Id,
                    LoginId = login.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return true;
        }

        public bool AddToRole(tbl_Users user, tbl_Roles role)
        {
            _context.Set<tbl_UserRoles>().Add(
                new tbl_UserRoles()
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return true;
        }

        public override tbl_Users Create(tbl_Users user)
        {
            var create = InternalCreate(user);

            _context.SaveChanges();

            return create;
        }

        public tbl_Users Create(tbl_Users user, string password)
        {
            var create = InternalCreate(user);

            _context.SaveChanges();

            SetPasswordHash(user, password);

            return create;
        }

        public override tbl_Users Delete(tbl_Users user)
        {
            var activity = _context.Set<tbl_Activities>()
                .Where(x => x.UserId == user.Id);

            var refreshes = _context.Set<tbl_Refreshes>()
                .Where(x => x.UserId == user.Id);

            var settings = _context.Set<tbl_Settings>()
                .Where(x => x.UserId == user.Id);

            var states = _context.Set<tbl_States>()
                .Where(x => x.UserId == user.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);

            return _context.Remove(user).Entity;
        }

        [Obsolete]
        public List<Claim> GenerateAccessClaims(tbl_Users user)
        {
            var legacyClaims = _context.Set<tbl_Settings>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (!string.IsNullOrEmpty(user.EmailAddress))
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

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

        public List<Claim> GenerateAccessClaims(tbl_Issuers issuer, tbl_Users user)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingAccessExpire).Single();

            var legacyClaims = _context.Set<tbl_Settings>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (!string.IsNullOrEmpty(user.EmailAddress))
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

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

        public List<Claim> GenerateRefreshClaims(tbl_Issuers issuer, tbl_Users user)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingRefreshExpire).Single();

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

        internal tbl_Users InternalCreate(tbl_Users user)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            return _context.Add(user).Entity;
        }

        public bool IsInClaim(tbl_Users user, tbl_Claims claim)
        {
            /*
             * TODO need to add check for role based claims...
             */

            if (_context.Set<tbl_UserClaims>()
                .Any(x => x.UserId == user.Id && x.ClaimId == claim.Id))
                return true;

            return false;
        }

        public bool IsInLogin(tbl_Users user, tbl_Logins login)
        {
            if (_context.Set<tbl_UserLogins>()
                .Any(x => x.UserId == user.Id && x.LoginId == login.Id))
                return true;

            return false;
        }

        public bool IsInRole(tbl_Users user, tbl_Roles role)
        {
            if (_context.Set<tbl_UserRoles>()
                .Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                return true;

            return false;
        }

        public bool IsLockedOut(tbl_Users user)
        {
            var entity = _context.Set<tbl_Users>()
                .Where(x => x.Id == user.Id).Single();

            if (entity.LockoutEnabled)
            {
                if (entity.LockoutEnd.HasValue && entity.LockoutEnd <= DateTime.UtcNow)
                {
                    entity.LockoutEnabled = false;
                    entity.LockoutEnd = null;

                    Update(entity);

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                Update(entity);

                return false;
            }
        }

        public bool IsPasswordSet(tbl_Users user)
        {
            var entity = _context.Set<tbl_Users>()
                .Where(x => x.Id == user.Id).Single();

            if (string.IsNullOrEmpty(entity.PasswordHashPBKDF2))
                return false;

            return true;
        }

        public bool RemoveFromClaim(tbl_Users user, tbl_Claims claim)
        {
            var entity = _context.Set<tbl_UserClaims>()
                .Where(x => x.UserId == user.Id && x.ClaimId == claim.Id).Single();

            _context.Set<tbl_UserClaims>().Remove(entity);

            return true;
        }

        public bool RemoveFromLogin(tbl_Users user, tbl_Logins login)
        {
            var entity = _context.Set<tbl_UserLogins>()
                .Where(x => x.UserId == user.Id && x.LoginId == login.Id).Single();

            _context.Set<tbl_UserLogins>().Remove(entity);

            return true;
        }

        public bool RemoveFromRole(tbl_Users user, tbl_Roles role)
        {
            var entity = _context.Set<tbl_UserRoles>()
                .Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.Set<tbl_UserRoles>().Remove(entity);

            return true;
        }

        public tbl_Users SetConfirmedEmail(tbl_Users user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_Users SetConfirmedPassword(tbl_Users user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_Users SetConfirmedPhoneNumber(tbl_Users user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_Users SetImmutable(tbl_Users user, bool enabled)
        {
            user.Immutable = enabled;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_Users SetPasswordHash(tbl_Users user, string password)
        {
            //https://www.google.com/search?q=identity+securitystamp
            if (!_context.Set<tbl_Users>()
                .Where(x => x.Id == user.Id && x.SecurityStamp == user.SecurityStamp)
                .Any())
                throw new InvalidOperationException();

            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.LastUpdated = Clock.UtcDateTime;

            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHashPBKDF2 = PBKDF2.Create(password);
                user.PasswordHashSHA256 = SHA256.Create(password);
            }

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public tbl_Users SetTwoFactorEnabled(tbl_Users user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            user.LastUpdated = Clock.UtcDateTime;

            _context.Entry(user).State = EntityState.Modified;

            return _context.Entry(user).Entity;
        }

        public override tbl_Users Update(tbl_Users user)
        {
            var entity = _context.Set<tbl_Users>()
                .Where(x => x.Id == user.Id).Single();

            //https://www.google.com/search?q=identity+concurrencystamp
            if (entity.ConcurrencyStamp != user.ConcurrencyStamp)
                throw new InvalidOperationException();

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

            return _context.Update(entity).Entity;
        }
    }
}