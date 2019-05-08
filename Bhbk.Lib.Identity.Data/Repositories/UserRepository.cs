﻿using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Data.Validators;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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

    public class UserRepository : IGenericRepositoryAsync<tbl_Users, Guid>
    {
        private readonly _DbContext _context;
        private readonly InstanceContext _instance;
        private IClockService _clock;
        public readonly PasswordValidator passwordValidator;
        public readonly PasswordHasher passwordHasher;
        public readonly UserValidator userValidator;

        public UserRepository(_DbContext context, InstanceContext instance)
        {
            _context = context ?? throw new NullReferenceException();
            _instance = instance;
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

        public async Task<bool> AccessFailedAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).SingleOrDefault();

            entity.LastLoginFailure = Clock.UtcDateTime;
            entity.AccessFailedCount++;

            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> AccessSuccessAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.LastLoginSuccess = Clock.UtcDateTime;
            entity.AccessSuccessCount++;

            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> AddToClaimAsync(tbl_Users user, tbl_Claims claim)
        {
            _context.tbl_UserClaims.Add(
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
            _context.tbl_UserLogins.Add(
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
            _context.tbl_UserRoles.Add(
                new tbl_UserRoles()
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    Created = Clock.UtcDateTime,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> CheckPasswordAsync(Guid key, string password)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            if (await InternalVerifyPasswordAsync(entity, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }

        public async Task<int> CountAsync(Expression<Func<tbl_Users, bool>> predicates = null)
        {
            var query = _context.tbl_Users.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<int> CountMOTDAsync(Expression<Func<tbl_MotDType1, bool>> predicates = null)
        {
            var query = _context.tbl_MotDType1.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<tbl_Users> CreateAsync(tbl_Users entity)
        {
            var create = await InternalCreateAsync(entity);

            _context.SaveChanges();

            return await Task.FromResult(create);
        }

        public async Task<tbl_Users> CreateAsync(tbl_Users entity, string password)
        {
            var create = await InternalCreateAsync(entity);

            _context.SaveChanges();

            await InternalSetPasswordAsync(entity, password);

            return await Task.FromResult(create);
        }

        public async Task<tbl_MotDType1> CreateMOTDAsync(tbl_MotDType1 model)
        {
            return await Task.FromResult(_context.Add(model).Entity);
        }

        public async Task<tbl_QueueEmails> CreateQueueEmailAsync(tbl_QueueEmails model)
        {
            return await Task.FromResult(_context.Add(model).Entity);
        }

        public async Task<tbl_QueueTexts> CreateQueueTextAsync(tbl_QueueTexts model)
        {
            return await Task.FromResult(_context.Add(model).Entity);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            var activity = _context.tbl_Activities.Where(x => x.UserId == key);
            var refreshes = _context.tbl_Refreshes.Where(x => x.UserId == key);
            var settings = _context.tbl_Settings.Where(x => x.UserId == key);
            var states = _context.tbl_States.Where(x => x.UserId == key);

            try
            {
                _context.RemoveRange(activity);
                _context.RemoveRange(refreshes);
                _context.RemoveRange(settings);
                _context.RemoveRange(states);
                _context.Remove(entity);

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeleteMOTDAsync(string key)
        {
            var entity = _context.tbl_MotDType1.Where(x => x.Id == key).Single();

            try
            {
                _context.Remove(entity);

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeleteQueueEmailAsync(string key)
        {
            var entity = _context.tbl_QueueEmails.Where(x => x.Id.ToString() == key).Single();

            try
            {
                _context.Remove(entity);

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeleteQueueTextAsync(string key)
        {
            var entity = _context.tbl_QueueTexts.Where(x => x.Id.ToString() == key).Single();

            try
            {
                _context.Remove(entity);

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.tbl_Users.Any(x => x.Id == key));
        }

        public async Task<ClaimsPrincipal> GenerateAccessClaimsAsync(tbl_Issuers issuer, tbl_Users user)
        {
            var expire = _context.tbl_Settings.Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingAccessExpire).Single();

            var legacyClaims = _context.tbl_Settings.Where(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
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

            foreach (var role in (await GetRolesAsync(user.Id)).ToList().OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                //check compatibility is enabled. pack claim(s) with old name and new name.
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            foreach (var claim in (await GetClaimsAsync(user.Id)).ToList().OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, RandomValues.CreateBase64String(8), ClaimValueTypes.String));

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
            var expire = _context.tbl_Settings.Where(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //defaults...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, RandomValues.CreateBase64String(8), ClaimValueTypes.String));

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

        public async Task<IEnumerable<tbl_Users>> GetAsync(Expression<Func<tbl_Users, bool>> predicates = null,
            Func<IQueryable<tbl_Users>, IIncludableQueryable<tbl_Users, object>> includes = null,
            Func<IQueryable<tbl_Users>, IOrderedQueryable<tbl_Users>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_Users.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<tbl_MotDType1>> GetMOTDAsync(Expression<Func<tbl_MotDType1, bool>> predicates = null,
            Func<IQueryable<tbl_MotDType1>, IIncludableQueryable<tbl_MotDType1, object>> includes = null,
            Func<IQueryable<tbl_MotDType1>, IOrderedQueryable<tbl_MotDType1>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_MotDType1.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<tbl_QueueEmails>> GetQueueEmailAsync(Expression<Func<tbl_QueueEmails, bool>> predicates = null,
            Func<IQueryable<tbl_QueueEmails>, IIncludableQueryable<tbl_QueueEmails, object>> includes = null,
            Func<IQueryable<tbl_QueueEmails>, IOrderedQueryable<tbl_QueueEmails>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_QueueEmails.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<tbl_QueueTexts>> GetQueueTextAsync(Expression<Func<tbl_QueueTexts, bool>> predicates = null,
            Func<IQueryable<tbl_QueueTexts>, IIncludableQueryable<tbl_QueueTexts, object>> includes = null,
            Func<IQueryable<tbl_QueueTexts>, IOrderedQueryable<tbl_QueueTexts>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.tbl_QueueTexts.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<tbl_Claims>> GetClaimsAsync(Guid key)
        {
            var result = _context.tbl_Claims.Where(x => x.tbl_UserClaims.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<tbl_Clients>> GetClientsAsync(Guid key)
        {
            var result = _context.tbl_Clients.Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == key))).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<tbl_Logins>> GetLoginsAsync(Guid key)
        {
            var result = _context.tbl_Logins.Where(x => x.tbl_UserLogins.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<tbl_Roles>> GetRolesAsync(Guid key)
        {
            var result = _context.tbl_Roles.Where(x => x.tbl_UserRoles.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        internal async Task<tbl_Users> InternalCreateAsync(tbl_Users entity)
        {
            if (!(await userValidator.ValidateAsync(entity)).Succeeded)
                throw new InvalidOperationException();

            if (!entity.HumanBeing)
                entity.EmailConfirmed = true;

            return await Task.FromResult(_context.Add(entity).Entity);
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
                || !await InternalSetSecurityStampAsync(user, RandomValues.CreateBase64String(32)))
                return false;

            return true;
        }

        internal async Task<PasswordVerificationResult> InternalVerifyPasswordAsync(tbl_Users model, string password)
        {
            if (passwordHasher == null)
                throw new NotSupportedException();

            if (passwordHasher.VerifyHashedPassword(model, model.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return await Task.FromResult(PasswordVerificationResult.Failed);
        }

        public async Task<bool> IsInClaimAsync(Guid userKey, Guid claimKey)
        {
            /*
             * TODO need to add check for role based claims...
             */

            if (_context.tbl_UserClaims.Any(x => x.UserId == userKey && x.ClaimId == claimKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInLoginAsync(Guid userKey, Guid loginKey)
        {
            if (_context.tbl_UserLogins.Any(x => x.UserId == userKey && x.LoginId == loginKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInRoleAsync(Guid userKey, Guid roleKey)
        {
            if (_context.tbl_UserRoles.Any(x => x.UserId == userKey && x.RoleId == roleKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsLockedOutAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).SingleOrDefault();

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
            var entity = _context.tbl_Users.Where(x => x.Id == key).SingleOrDefault();

            if (entity == null)
                return await Task.FromResult(false);

            else if (string.IsNullOrEmpty(entity.PasswordHash))
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromClaimAsync(tbl_Users user, tbl_Claims claim)
        {
            var entity = _context.tbl_UserClaims.Where(x => x.UserId == user.Id && x.ClaimId == claim.Id).Single();

            _context.tbl_UserClaims.Remove(entity);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromLoginAsync(tbl_Users user, tbl_Logins login)
        {
            var entity = _context.tbl_UserLogins.Where(x => x.UserId == user.Id && x.LoginId == login.Id).Single();

            _context.tbl_UserLogins.Remove(entity);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromRoleAsync(tbl_Users user, tbl_Roles role)
        {
            var entity = _context.tbl_UserRoles.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.tbl_UserRoles.Remove(entity);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemovePasswordAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            return await InternalSetPasswordHashAsync(entity, null);
        }

        public async Task<bool> SetConfirmedEmailAsync(Guid key, bool confirmed)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.EmailConfirmed = confirmed;
            entity.LastUpdated = Clock.UtcDateTime;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPasswordAsync(Guid key, bool confirmed)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.PasswordConfirmed = confirmed;
            entity.LastUpdated = Clock.UtcDateTime;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPhoneNumberAsync(Guid key, bool confirmed)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.PhoneNumberConfirmed = confirmed;
            entity.LastUpdated = Clock.UtcDateTime;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetImmutableAsync(Guid key, bool enabled)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.Immutable = enabled;
            entity.LastUpdated = Clock.UtcDateTime;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetPasswordAsync(Guid key, string password)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            return await InternalSetPasswordAsync(entity, password);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(Guid key, bool enabled)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.TwoFactorEnabled = enabled;
            entity.LastUpdated = Clock.UtcDateTime;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<tbl_Users> UpdateAsync(tbl_Users model)
        {
            if (!(await userValidator.ValidateAsync(model)).Succeeded)
                throw new InvalidOperationException();

            var entity = _context.tbl_Users.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.FirstName = model.FirstName;
            entity.LastName = model.LastName;
            entity.LockoutEnabled = model.LockoutEnabled;
            entity.LockoutEnd = model.LockoutEnd.HasValue ? model.LockoutEnd.Value.ToUniversalTime() : model.LockoutEnd;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }

        public async Task<tbl_MotDType1> UpdateAsync(tbl_MotDType1 model)
        {
            var entity = _context.tbl_MotDType1.Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}