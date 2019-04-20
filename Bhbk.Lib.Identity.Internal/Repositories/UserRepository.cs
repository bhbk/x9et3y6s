using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Core.Repositories;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Validators;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
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

    public class UserRepository : IGenericRepositoryAsync<UserCreate, tbl_Users, Guid>
    {
        private readonly InstanceContext _instance;
        private readonly IConfigurationRoot _conf;
        private readonly IMapper _mapper;
        private readonly IdentityDbContext _context;
        private readonly PasswordValidator _passwordValidator;
        public readonly PasswordHasher passwordHasher;
        public readonly UserValidator userValidator;

        public UserRepository(IdentityDbContext context, InstanceContext instance, IConfigurationRoot conf, IMapper mapper)
        {
            _context = context;
            _instance = instance;
            _conf = conf;
            _mapper = mapper;
            _passwordValidator = new PasswordValidator();

            passwordHasher = new PasswordHasher();
            userValidator = new UserValidator();
        }

        public async Task<bool> AccessFailedAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).SingleOrDefault();

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
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

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
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            if (!string.IsNullOrEmpty(entity.PasswordHash))
                throw new InvalidOperationException();

            return await UpdatePassword(entity, password);
        }

        public async Task<bool> AddToClaimAsync(tbl_Users user, tbl_Claims claim)
        {
            _context.tbl_UserClaims.Add(
                new tbl_UserClaims()
                {
                    UserId = user.Id,
                    ClaimId = claim.Id,
                    Created = DateTime.Now,
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
                    Created = DateTime.Now,
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
                    Created = DateTime.Now,
                    Immutable = false
                });

            return await Task.FromResult(true);
        }

        public async Task<bool> CheckPasswordAsync(Guid key, string password)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            if (await VerifyPasswordAsync(entity, password) != PasswordVerificationResult.Failed)
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

        internal async Task<tbl_Users> CreateAsync(tbl_Users model)
        {
            if (!(await userValidator.ValidateAsync(model)).Succeeded)
                throw new InvalidOperationException();

            if (!model.HumanBeing)
                model.EmailConfirmed = true;

            return await Task.FromResult(_context.Add(model).Entity);
        }

        public async Task<tbl_Users> CreateAsync(UserCreate model)
        {
            var entity = _mapper.Map<tbl_Users>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            return await Task.FromResult(create);
        }

        public async Task<tbl_Users> CreateAsync(UserCreate model, string password)
        {
            var entity = _mapper.Map<tbl_Users>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            await UpdatePassword(entity, password);

            return await Task.FromResult(create);
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            var activity = _context.tbl_Activities.Where(x => x.UserId == key);
            var refreshes = _context.tbl_Refreshes.Where(x => x.UserId == key);
            var states = _context.tbl_States.Where(x => x.UserId == key);

            try
            {
                _context.RemoveRange(activity);
                _context.RemoveRange(refreshes);
                _context.RemoveRange(states);

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

        public async Task<ClaimsPrincipal> GenerateAccessTokenAsync(tbl_Users user)
        {
            var claims = new List<Claim>();

            //user identity vs. a service identity
            claims.Add(new Claim(ClaimTypes.System, ClientType.user_agent.ToString()));

            foreach (var role in (await GetRolesAsync(user.Id)).ToList().OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                //check compatibility is enabled. pack claim(s) with old name and new name.
                if (bool.Parse(_conf["IdentityDefaults:LegacyModeClaims"]))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            foreach (var claim in (await GetClaimsAsync(user.Id)).ToList().OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:ResourceOwnerTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }

        public async Task<ClaimsPrincipal> GenerateRefreshTokenAsync(tbl_Users user)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:ResourceOwnerRefreshExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

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

            //query = query.Include(x => x.UserRoles);

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
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();
            var result = _context.tbl_Claims.Where(x => x.tbl_UserClaims.Any(y => y.UserId == key)).ToList();

            result.Add(new tbl_Claims() { Id = Guid.NewGuid(), Type = ClaimTypes.NameIdentifier, Value = entity.Id.ToString() });
            result.Add(new tbl_Claims() { Id = Guid.NewGuid(), Type = ClaimTypes.Email, Value = entity.Email });
            result.Add(new tbl_Claims() { Id = Guid.NewGuid(), Type = ClaimTypes.MobilePhone, Value = entity.PhoneNumber });
            result.Add(new tbl_Claims() { Id = Guid.NewGuid(), Type = ClaimTypes.GivenName, Value = entity.FirstName });
            result.Add(new tbl_Claims() { Id = Guid.NewGuid(), Type = ClaimTypes.Surname, Value = entity.LastName });

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

                    await UpdateAsync(_mapper.Map<tbl_Users>(entity));

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                await UpdateAsync(_mapper.Map<tbl_Users>(entity));

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
            var result = _context.tbl_UserClaims.Where(x => x.UserId == user.Id && x.ClaimId == claim.Id).Single();

            _context.tbl_UserClaims.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromLoginAsync(tbl_Users user, tbl_Logins login)
        {
            var result = _context.tbl_UserLogins.Where(x => x.UserId == user.Id && x.LoginId == login.Id).Single();

            _context.tbl_UserLogins.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFromRoleAsync(tbl_Users user, tbl_Roles role)
        {
            var result = _context.tbl_UserRoles.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.tbl_UserRoles.Remove(result);

            return await Task.FromResult(true);
        }

        public async Task<bool> RemovePasswordAsync(Guid key)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            return await SetPasswordHashAsync(entity, null);
        }

        public async Task<bool> SetConfirmedEmailAsync(Guid key, bool confirmed)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.EmailConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPasswordAsync(Guid key, bool confirmed)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.PasswordConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPhoneNumberAsync(Guid key, bool confirmed)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.PhoneNumberConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetImmutableAsync(Guid key, bool enabled)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.Immutable = enabled;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetPasswordHashAsync(tbl_Users user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetSecurityStampAsync(tbl_Users user, string stamp)
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(Guid key, bool enabled)
        {
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            entity.TwoFactorEnabled = enabled;
            entity.LastUpdated = DateTime.Now;

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
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }

        internal async Task<bool> UpdatePassword(tbl_Users user, string password)
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
            var entity = _context.tbl_Users.Where(x => x.Id == key).Single();

            if (!await UpdatePassword(entity, password))
                return false;

            return true;
        }

        internal async Task<PasswordVerificationResult> VerifyPasswordAsync(tbl_Users user, string password)
        {
            if (passwordHasher == null)
                throw new NotSupportedException();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return await Task.FromResult(PasswordVerificationResult.Failed);
        }
    }
}