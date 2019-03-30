using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : IGenericRepository<UserCreate, TUsers, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IConfigurationRoot _conf;
        private readonly IMapper _transform;
        private readonly DatabaseContext _context;
        private readonly PasswordValidator _passwordValidator;
        public readonly PasswordHasher passwordHasher;
        public readonly UserValidator userValidator;

        public UserRepository(DatabaseContext context, ExecutionType situation, IConfigurationRoot conf, IMapper transform)
        {
            _context = context;
            _situation = situation;
            _conf = conf;
            _transform = transform;
            _passwordValidator = new PasswordValidator();

            passwordHasher = new PasswordHasher();
            userValidator = new UserValidator();
        }

        public async Task<bool> AccessFailedAsync(Guid key)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).SingleOrDefault();

            entity.LastLoginFailure = DateTime.Now;
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
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            entity.LastLoginSuccess = DateTime.Now;
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

        public async Task<bool> AddPasswordAsync(Guid key, string password)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            if (!string.IsNullOrEmpty(entity.PasswordHash))
                throw new InvalidOperationException();

            return await UpdatePassword(entity, password);
        }

        public async Task<bool> AddToClaimAsync(TUsers user, TClaims claim)
        {
            _context.TUserClaims.Add(
                new TUserClaims()
                {
                    UserId = user.Id,
                    ClaimId = claim.Id,
                    Created = DateTime.Now,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> AddToLoginAsync(TUsers user, TLogins login)
        {
            _context.TUserLogins.Add(
                new TUserLogins()
                {
                    UserId = user.Id,
                    LoginId = login.Id,
                    Created = DateTime.Now,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> AddToRoleAsync(TUsers user, TRoles role)
        {
            _context.TUserRoles.Add(
                new TUserRoles()
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    Created = DateTime.Now,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> CheckPasswordAsync(Guid key, string password)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            if (await VerifyPasswordAsync(entity, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }

        public async Task<int> CountAsync(Expression<Func<TUsers, bool>> predicates = null)
        {
            var query = _context.TUsers.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        internal async Task<TUsers> CreateAsync(TUsers user)
        {
            var check = await userValidator.ValidateAsync(user);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            return await Task.FromResult(_context.Add(user).Entity);
        }

        public async Task<TUsers> CreateAsync(UserCreate model)
        {
            var entity = _transform.Map<TUsers>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            return await Task.FromResult(create);
        }

        public async Task<TUsers> CreateAsync(UserCreate model, string password)
        {
            var entity = _transform.Map<TUsers>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            await UpdatePassword(entity, password);

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            var activity = _context.TActivities.Where(x => x.ActorId == key);
            var claims = _context.TClaims.Where(x => x.ActorId == key);
            var logins = _context.TLogins.Where(x => x.ActorId == key);
            var refreshes = _context.TRefreshes.Where(x => x.UserId == key);

            try
            {
                _context.RemoveRange(activity);
                _context.RemoveRange(claims);
                _context.RemoveRange(logins);
                _context.RemoveRange(refreshes);

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
            return await Task.FromResult(_context.TUsers.Any(x => x.Id == key));
        }

        public async Task<ClaimsPrincipal> GenerateAccessTokenAsync(TUsers user)
        {
            /*
             * moving away from microsoft constructs for identity implementation because of un-needed additional 
             * layers of complexity, and limitations, for the simple operations needing to be performed.
             * 
             * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iuserclaimsprincipalfactory-1
             */

            var claims = new List<Claim>();

            foreach (var role in (await GetRolesAsync(user.Id)).ToList().OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            //check if claims compatibility enabled. means pack claim(s) with old name too.
            if (bool.Parse(_conf["IdentityDefaults:CompatibilityModeClaims"]))
                foreach (var role in claims.Where(x => x.Type == ClaimTypes.Role).ToList().OrderBy(x => x.Type))
                    claims.Add(new Claim("role", role.Value, ClaimTypes.Role));

            foreach (TClaims claim in (await GetClaimsAsync(user.Id)).ToList().OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:AccessTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<ClaimsPrincipal> GenerateRefreshTokenAsync(TUsers user)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:RefreshTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<IEnumerable<TUsers>> GetAsync(Expression<Func<TUsers, bool>> predicates = null,
            Func<IQueryable<TUsers>, IIncludableQueryable<TUsers, object>> includes = null,
            Func<IQueryable<TUsers>, IOrderedQueryable<TUsers>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.TUsers.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.UserRoles);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        //public async Task<IEnumerable<Refreshes>> GetRefreshAsync(Expression<Func<Refreshes, bool>> predicates = null,
        //    Func<IQueryable<Refreshes>, IIncludableQueryable<Refreshes, object>> includes = null,
        //    Func<IQueryable<Refreshes>, IOrderedQueryable<Refreshes>> orderBy = null)
        //{
        //    var query = _context.Refreshes.AsQueryable();

        //    if (predicates != null)
        //        query = query.Where(predicates);

        //    if (includes != null)
        //        query = includes(query);

        //    if (orderBy != null)
        //        return await Task.FromResult(orderBy(query));

        //    return await Task.FromResult(query);
        //}

        public async Task<IEnumerable<TClaims>> GetClaimsAsync(Guid key)
        {
            var result = _context.TClaims.Where(x => x.TUserClaims.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<TClients>> GetClientsAsync(Guid key)
        {
            var result = _context.TClients.Where(x => x.TRoles.Any(y => y.TUserRoles.Any(z => z.UserId == key))).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<TLogins>> GetLoginsAsync(Guid key)
        {
            var result = _context.TLogins.Where(x => x.TUserLogins.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<TRoles>> GetRolesAsync(Guid key)
        {
            var result = _context.TRoles.Where(x => x.TUserRoles.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<bool> IsInClaimAsync(Guid userKey, Guid claimKey)
        {
            /*
             * TODO need to add check for role based claims...
             */

            if (_context.TUserClaims.Any(x => x.UserId == userKey && x.ClaimId == claimKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInLoginAsync(Guid userKey, Guid loginKey)
        {
            if (_context.TUserLogins.Any(x => x.UserId == userKey && x.LoginId == loginKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInRoleAsync(Guid userKey, Guid roleKey)
        {
            if (_context.TUserRoles.Any(x => x.UserId == userKey && x.RoleId == roleKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsLockedOutAsync(Guid key)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).SingleOrDefault();

            if (entity.LockoutEnabled)
            {
                if (entity.LockoutEnd.HasValue && entity.LockoutEnd <= DateTime.UtcNow)
                {
                    entity.LockoutEnabled = false;
                    entity.LockoutEnd = null;

                    await UpdateAsync(_transform.Map<TUsers>(entity));

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                await UpdateAsync(_transform.Map<TUsers>(entity));

                return false;
            }
        }

        public async Task<bool> IsPasswordSetAsync(Guid key)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).SingleOrDefault();

            if (entity == null)
                return await Task.FromResult(false);

            else if (string.IsNullOrEmpty(entity.PasswordHash))
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromClaimAsync(TUsers user, TClaims claim)
        {
            var result = _context.TUserClaims.Where(x => x.UserId == user.Id && x.ClaimId == claim.Id).Single();

            _context.TUserClaims.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromLoginAsync(TUsers user, TLogins login)
        {
            var result = _context.TUserLogins.Where(x => x.UserId == user.Id && x.LoginId == login.Id).Single();

            _context.TUserLogins.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromRoleAsync(TUsers user, TRoles role)
        {
            var result = _context.TUserRoles.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.TUserRoles.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemovePasswordAsync(Guid key)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            return await SetPasswordHashAsync(entity, null);
        }

        public async Task<bool> SetConfirmedEmailAsync(Guid key, bool confirmed)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            entity.EmailConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPasswordAsync(Guid key, bool confirmed)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            entity.PasswordConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPhoneNumberAsync(Guid key, bool confirmed)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            entity.PhoneNumberConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetImmutableAsync(Guid key, bool enabled)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            entity.Immutable = enabled;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetPasswordHashAsync(TUsers user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetSecurityStampAsync(TUsers user, string stamp)
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(Guid key, bool enabled)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            entity.TwoFactorEnabled = enabled;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<TUsers> UpdateAsync(TUsers model)
        {
            var user = _transform.Map<TUsers>(model);
            var check = await userValidator.ValidateAsync(user);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            var entity = _context.TUsers.Where(x => x.Id == user.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            entity.LockoutEnabled = user.LockoutEnabled;
            entity.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = user.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }

        internal async Task<bool> UpdatePassword(TUsers user, string password)
        {
            if (_passwordValidator == null)
                throw new NotSupportedException();

            var result = await _passwordValidator.ValidateAsync(user, password);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            if (passwordHasher == null)
                throw new NotSupportedException();

            var hash = passwordHasher.HashPassword(user, password);

            if (!await SetPasswordHashAsync(user, hash)
                || !await SetSecurityStampAsync(user, RandomValues.CreateBase64String(32)))
                return false;

            return true;
        }

        public async Task<bool> UpdatePassword(Guid key, string password)
        {
            var entity = _context.TUsers.Where(x => x.Id == key).Single();

            if (!await UpdatePassword(entity, password))
                return false;

            return true;
        }

        internal async Task<PasswordVerificationResult> VerifyPasswordAsync(TUsers user, string password)
        {
            if (passwordHasher == null)
                throw new NotSupportedException();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return await Task.FromResult(PasswordVerificationResult.Failed);
        }
    }
}