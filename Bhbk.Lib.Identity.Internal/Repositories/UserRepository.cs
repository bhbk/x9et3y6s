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

    public class UserRepository : IGenericRepository<UserCreate, AppUser, Guid>
    {
        private readonly ExecutionType _situation;
        private readonly IConfigurationRoot _conf;
        private readonly IMapper _transform;
        private readonly AppDbContext _context;
        private readonly PasswordValidator _passwordValidator;
        public readonly PasswordHasher passwordHasher;
        public readonly UserValidator userValidator;

        public UserRepository(AppDbContext context, ExecutionType situation, IConfigurationRoot conf, IMapper transform)
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
            var entity = _context.AppUser.Where(x => x.Id == key).SingleOrDefault();

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
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

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
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            if (!string.IsNullOrEmpty(entity.PasswordHash))
                throw new InvalidOperationException();

            return await UpdatePassword(entity, password);
        }

        public async Task<bool> AddToClaimAsync(AppUser user, AppClaim claim)
        {
            _context.AppUserClaim.Add(
                new AppUserClaim()
                {
                    UserId = user.Id,
                    ClaimId = claim.Id,
                    Created = DateTime.Now,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> AddToLoginAsync(AppUser user, AppLogin login)
        {
            _context.AppUserLogin.Add(
                new AppUserLogin()
                {
                    UserId = user.Id,
                    LoginId = login.Id,
                    Created = DateTime.Now,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> AddToRoleAsync(AppUser user, AppRole role)
        {
            _context.AppUserRole.Add(
                new AppUserRole()
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
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            if (await VerifyPasswordAsync(entity, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }

        public async Task<int> CountAsync(Expression<Func<AppUser, bool>> predicates = null)
        {
            var query = _context.AppUser.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        internal async Task<AppUser> CreateAsync(AppUser user)
        {
            var check = await userValidator.ValidateAsync(user);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            return await Task.FromResult(_context.Add(user).Entity);
        }

        public async Task<AppUser> CreateAsync(UserCreate model)
        {
            var entity = _transform.Map<AppUser>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            return await Task.FromResult(create);
        }

        public async Task<AppUser> CreateAsync(UserCreate model, string password)
        {
            var entity = _transform.Map<AppUser>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            await UpdatePassword(entity, password);

            return await Task.FromResult(create);
        }

        public async Task<AppUserRefresh> CreateRefreshAsync(UserRefreshCreate model)
        {
            var entity = _transform.Map<AppUserRefresh>(model);
            var create = _context.Add(entity).Entity;
        
            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            var activity = _context.AppActivity.Where(x => x.ActorId == key);
            var claims = _context.AppClaim.Where(x => x.ActorId == key);
            var logins = _context.AppLogin.Where(x => x.ActorId == key);

            try
            {
                _context.RemoveRange(activity);
                _context.RemoveRange(claims);
                _context.RemoveRange(logins);

                await Task.FromResult(_context.Remove(entity).Entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppUser.Any(x => x.Id == key));
        }

        public async Task<ClaimsPrincipal> GenerateAccessTokenAsync(AppUser user)
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

            foreach (AppClaim claim in (await GetClaimsAsync(user.Id)).ToList().OrderBy(x => x.Type))
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

        public async Task<ClaimsPrincipal> GenerateRefreshTokenAsync(AppUser user)
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

        public async Task<IEnumerable<AppUser>> GetAsync(Expression<Func<AppUser, bool>> predicates = null,
            Func<IQueryable<AppUser>, IIncludableQueryable<AppUser, object>> includes = null,
            Func<IQueryable<AppUser>, IOrderedQueryable<AppUser>> orders = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppUser.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            //query = query.Include(x => x.AppUserRole);

            if (orders != null)
            {
                query = orders(query)
                    .Skip(skip.Value)
                    .Take(take.Value);
            }

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<AppUserRefresh>> GetRefreshAsync(Expression<Func<AppUserRefresh, bool>> predicates = null,
            Func<IQueryable<AppUserRefresh>, IIncludableQueryable<AppUserRefresh, object>> includes = null,
            Func<IQueryable<AppUserRefresh>, IOrderedQueryable<AppUserRefresh>> orderBy = null)
        {
            var query = _context.AppUserRefresh.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<IEnumerable<AppClaim>> GetClaimsAsync(Guid key)
        {
            var result = _context.AppClaim.Where(x => x.AppUserClaim.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<AppClient>> GetClientsAsync(Guid key)
        {
            var result = _context.AppClient.Where(x => x.AppRole.Any(y => y.AppUserRole.Any(z => z.UserId == key))).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<AppLogin>> GetLoginsAsync(Guid key)
        {
            var result = _context.AppLogin.Where(x => x.AppUserLogin.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<AppRole>> GetRolesAsync(Guid key)
        {
            var result = _context.AppRole.Where(x => x.AppUserRole.Any(y => y.UserId == key)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<bool> IsInClaimAsync(Guid userKey, Guid claimKey)
        {
            /*
             * TODO need to add check for role based claims...
             */

            if (_context.AppUserClaim.Any(x => x.UserId == userKey && x.ClaimId == claimKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInLoginAsync(Guid userKey, Guid loginKey)
        {
            if (_context.AppUserLogin.Any(x => x.UserId == userKey && x.LoginId == loginKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInRoleAsync(Guid userKey, Guid roleKey)
        {
            if (_context.AppUserRole.Any(x => x.UserId == userKey && x.RoleId == roleKey))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsLockedOutAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).SingleOrDefault();

            if (entity.LockoutEnabled)
            {
                if (entity.LockoutEnd.HasValue && entity.LockoutEnd <= DateTime.UtcNow)
                {
                    entity.LockoutEnabled = false;
                    entity.LockoutEnd = null;

                    await UpdateAsync(_transform.Map<AppUser>(entity));

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                await UpdateAsync(_transform.Map<AppUser>(entity));

                return false;
            }
        }

        public async Task<bool> IsPasswordSetAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).SingleOrDefault();

            if (entity == null)
                return await Task.FromResult(false);

            else if (string.IsNullOrEmpty(entity.PasswordHash))
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromClaimAsync(AppUser user, AppClaim claim)
        {
            var result = _context.AppUserClaim.Where(x => x.UserId == user.Id && x.ClaimId == claim.Id).Single();

            _context.AppUserClaim.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromLoginAsync(AppUser user, AppLogin login)
        {
            var result = _context.AppUserLogin.Where(x => x.UserId == user.Id && x.LoginId == login.Id).Single();

            _context.AppUserLogin.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromRoleAsync(AppUser user, AppRole role)
        {
            var result = _context.AppUserRole.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.AppUserRole.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveRefreshTokenAsync(Guid key)
        {
            var token = _context.AppUserRefresh.Where(x => x.Id == key);

            if (token == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(token);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveRefreshTokensAsync(AppUser user)
        {
            var tokens = _context.AppUserRefresh.Where(x => x.UserId == user.Id);

            if (tokens == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(tokens);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemovePasswordAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            return await SetPasswordHashAsync(entity, null);
        }

        public async Task<bool> SetConfirmedEmailAsync(Guid key, bool confirmed)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.EmailConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPasswordAsync(Guid key, bool confirmed)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.PasswordConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPhoneNumberAsync(Guid key, bool confirmed)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.PhoneNumberConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetImmutableAsync(Guid key, bool enabled)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.Immutable = enabled;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetPasswordHashAsync(AppUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetSecurityStampAsync(AppUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(Guid key, bool enabled)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.TwoFactorEnabled = enabled;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<AppUser> UpdateAsync(AppUser model)
        {
            var user = _transform.Map<AppUser>(model);
            var check = await userValidator.ValidateAsync(user);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            var entity = _context.AppUser.Where(x => x.Id == user.Id).Single();

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

            return await Task.FromResult(_transform.Map<AppUser>(entity));
        }

        internal async Task<bool> UpdatePassword(AppUser user, string password)
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
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            if (!await UpdatePassword(entity, password))
                return false;

            return true;
        }

        internal async Task<PasswordVerificationResult> VerifyPasswordAsync(AppUser user, string password)
        {
            if (passwordHasher == null)
                throw new NotSupportedException();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return await Task.FromResult(PasswordVerificationResult.Failed);
        }
    }
}