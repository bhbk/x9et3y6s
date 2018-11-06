using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.EntityModels;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : IGenericRepository<AppUser, Guid>
    {
        private readonly ContextType _situation;
        private readonly AppDbContext _context;
        private readonly PasswordValidator _pv;
        public readonly UserClaimFactory claimProvider;
        public readonly PasswordHasher passwordHasher;
        public readonly UserValidator userValidator;

        public UserRepository(AppDbContext context, ContextType situation, IConfigurationRoot conf)
        {
            _context = context;
            _situation = situation;
            _pv = new PasswordValidator();

            claimProvider = new UserClaimFactory(this, conf);
            passwordHasher = new PasswordHasher();
            userValidator = new UserValidator();
        }

        public async Task<AppUser> AccessFailedAsync(AppUser user)
        {
            user.LastLoginFailure = DateTime.Now;
            user.AccessFailedCount++;

            return await UpdateAsync(user);
        }

        public async Task<AppUser> AccessSuccessAsync(AppUser user)
        {
            user.LastLoginSuccess = DateTime.Now;
            user.AccessSuccessCount++;

            return await UpdateAsync(user);
        }

        public async Task<IdentityResult> AddClaimAsync(AppUser user, Claim claims)
        {
            var list = new List<Claim>();
            list.Add(claims);

            foreach (Claim entry in list)
            {
                var model = new AppUserClaim()
                {
                    UserId = user.Id,
                    ClaimType = entry.Type,
                    ClaimValue = entry.Value,
                    ClaimValueType = entry.ValueType
                };

                _context.AppUserClaim.Add(model);
            }

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddLoginAsync(AppUser user, UserLoginInfo info)
        {
            var login = _context.AppLogin.Where(x => x.LoginProvider == info.LoginProvider).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            AppUserLogin result = new AppUserLogin()
            {
                UserId = user.Id,
                LoginId = login.Id,
                LoginProvider = info.LoginProvider,
                ProviderDisplayName = info.ProviderDisplayName,
                ProviderDescription = info.ProviderDisplayName,
                ProviderKey = info.ProviderKey,
                Created = DateTime.Now,
                Enabled = true,
                Immutable = false
            };

            _context.AppUserLogin.Add(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddPasswordAsync(AppUser user, string password)
        {
            if (!string.IsNullOrEmpty(user.PasswordHash))
                throw new InvalidOperationException();

            return await UpdatePassword(user, password);
        }

        public async Task<IdentityResult> AddRefreshTokenAsync(AppUserRefresh refresh)
        {
            var list = _context.AppUserRefresh.Where(x => x.UserId == refresh.UserId
                && x.IssuedUtc > DateTime.UtcNow
                && x.ExpiresUtc < DateTime.UtcNow);

            if (list != null)
                _context.AppUserRefresh.RemoveRange(list);

            _context.AppUserRefresh.Add(refresh);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddToRoleAsync(AppUser user, string roleName)
        {
            var role = _context.AppRole.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            AppUserRole result = new AppUserRole()
            {
                UserId = user.Id,
                RoleId = role.Id,
                Created = DateTime.Now,
                Immutable = false
            };

            _context.AppUserRole.Add(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddToRolesAsync(AppUser user, IEnumerable<string> roles)
        {
            foreach (string role in roles)
                await AddToRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            if (await VerifyPasswordAsync(user, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }

        public async Task<int> Count(Expression<Func<AppUser, bool>> predicates = null)
        {
            var query = _context.AppUser.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public async Task<AppUser> CreateAsync(AppUser user)
        {
            var check = await userValidator.ValidateAsync(this, user);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            user.SecurityStamp = RandomValues.CreateBase64String(32);

            return await Task.FromResult(_context.AppUser.Add(user).Entity);
        }

        public async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            var check = await userValidator.ValidateAsync(this, user);

            if (!check.Succeeded)
                return check;

            _context.AppUser.Add(user);
            _context.SaveChanges();

            return await UpdatePassword(user, password);
        }

        public async Task<bool> DeleteAsync(AppUser user)
        {
            await Task.FromResult(_context.Remove(user).Entity);

            return true;
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppUser.Any(x => x.Id == key));
        }

        public Task<IQueryable<AppUser>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<AppUser>> GetAsync(Expression<Func<AppUser, bool>> predicates = null,
            Func<IQueryable<AppUser>, IQueryable<AppUser>> orderBy = null,
            Func<IQueryable<AppUser>, IIncludableQueryable<AppUser, object>> includes = null)
        {
            var query = _context.AppUser.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            query.Include("AppUserRole.Role").Load();

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<IQueryable<AppUserRefresh>> GetRefreshTokensAsync(Expression<Func<AppUserRefresh, bool>> predicates = null,
            Func<IQueryable<AppUserRefresh>, IQueryable<AppUserRefresh>> orderBy = null,
            Func<IQueryable<AppUserRefresh>, IIncludableQueryable<AppUserRefresh, object>> includes = null)
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

        public async Task<IList<Claim>> GetClaimsAsync(AppUser user)
        {
            var result = new List<Claim>();
            var map = _context.AppUserClaim.Where(x => x.UserId == user.Id);

            if (map == null)
                throw new InvalidOperationException();

            foreach (var claim in map)
            {
                var model = new Claim(claim.ClaimType,
                    claim.ClaimValue,
                    claim.ClaimValueType);

                result.Add(model);
            }

            return await Task.FromResult(result);
        }

        public async Task<IList<string>> GetClientsAsync(AppUser user)
        {
            var result = (IList<string>)_context.AppClient
                .Join(_context.AppRole, x => x.Id, y => y.ClientId, (client1, role1) => new {
                    ClientId = client1.Id,
                    RoleId = role1.Id
                })
                .Join(_context.AppUserRole, x => x.RoleId, y => y.RoleId, (role2, user2) => new {
                    ClientId = role2.ClientId,
                    UserId = user2.UserId
                })
                .Where(x => x.UserId == user.Id)
                .Select(x => x.ClientId.ToString().ToLower())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        public async Task<IList<string>> GetLoginsAsync(AppUser user)
        {
            var result = (IList<string>)_context.AppLogin
                .Join(_context.AppUserLogin, x => x.Id, y => y.LoginId, (login1, user1) => new {
                    LoginId = login1.Id,
                    UserId = user1.UserId
                })
                .Where(x => x.UserId == user.Id)
                .Select(x => x.LoginId.ToString().ToLower())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        public async Task<IList<string>> GetRolesAsync(AppUser user)
        {
            var result = (IList<string>)_context.AppRole
                .Join(_context.AppUserRole, x => x.Id, y => y.RoleId, (role1, user1) => new {
                    UserId = user1.UserId,
                    RoleId = role1.Id,
                    RoleName = role1.Name
                })
                .Where(x => x.UserId == user.Id)
                .Select(x => x.RoleName.ToString())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        [System.Obsolete]
        public async Task<IList<string>> GetRolesAsync_Deprecate(AppUser user)
        {
            var result = (IList<string>)_context.AppRole
                .Join(_context.AppUserRole, x => x.Id, y => y.RoleId, (role1, user1) => new {
                    UserId = user1.UserId,
                    RoleId = role1.Id,
                    RoleName = role1.Name
                })
                .Where(x => x.UserId == user.Id)
                .Select(x => x.RoleId.ToString())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        public async Task<bool> IsInLoginAsync(AppUser user, string loginName)
        {
            var login = _context.AppLogin.Where(x => x.LoginProvider == loginName).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            else if (_context.AppUserLogin.Any(x => x.UserId == user.Id && x.LoginId == login.Id))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInRoleAsync(AppUser user, string roleName)
        {
            var role = _context.AppRole.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else if (_context.AppUserRole.Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsLockedOutAsync(AppUser user)
        {
            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd.HasValue && user.LockoutEnd <= DateTime.UtcNow)
                {
                    user.LockoutEnabled = false;
                    user.LockoutEnd = null;

                    await UpdateAsync(user);

                    return false;
                }
                else
                    return true;
            }
            else
            {
                user.LockoutEnd = null;
                await UpdateAsync(user);

                return false;
            }
        }

        public async Task<bool> IsPasswordSetAsync(AppUser user)
        {
            var result = _context.AppUser.Where(x => x.Id == user.Id).SingleOrDefault();

            if (result == null)
                return await Task.FromResult(false);

            else if (string.IsNullOrEmpty(result.PasswordHash))
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        public async Task<IdentityResult> RemoveClaimAsync(AppUser user, Claim claim)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(claim);

            foreach (Claim entry in claims)
            {
                var result = _context.AppUserClaim.Where(x => x.UserId == user.Id
                    && x.ClaimType == entry.Type
                    && x.ClaimValue == entry.Value).SingleOrDefault();

                if (result == null)
                    throw new ArgumentNullException();

                _context.AppUserClaim.Remove(result);
            }

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveLoginAsync(AppUser user, string loginProvider, string providerKey)
        {
            var login = _context.AppUserLogin.Where(x => x.LoginProvider == loginProvider).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            var result = _context.AppUserLogin.Where(x => x.UserId == user.Id && x.LoginId == login.LoginId).Single();

            _context.AppUserLogin.Remove(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(AppUser user, string roleName)
        {
            var role = _context.Roles.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            var result = _context.AppUserRole.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.AppUserRole.Remove(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(AppUser user, IEnumerable<string> roles)
        {
            foreach (string role in roles)
                await RemoveFromRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveRefreshTokenAsync(AppUserRefresh refresh)
        {
            var token = _context.AppUserRefresh.Where(x => x.Id == refresh.Id);

            if (token == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(token);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveRefreshTokensAsync(AppUser user)
        {
            var tokens = _context.AppUserRefresh.Where(x => x.UserId == user.Id);

            if (tokens == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(tokens);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemovePasswordAsync(AppUser user)
        {
            await SetPasswordHashAsync(user, null);

            return IdentityResult.Success;
        }

        public async Task<AppUser> SetConfirmedEmailAsync(AppUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(user).Entity);
        }

        public async Task<AppUser> SetConfirmedPasswordAsync(AppUser user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(user).Entity);
        }

        public async Task<AppUser> SetConfirmedPhoneNumberAsync(AppUser user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(user).Entity);
        }

        public async Task<AppUser> SetImmutableAsync(AppUser user, bool enabled)
        {
            user.Immutable = enabled;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(user).Entity);
        }

        protected async Task<AppUser> SetPasswordHashAsync(AppUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(user).Entity);
        }

        protected async Task<AppUser> SetSecurityStampAsync(AppUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(user).Entity);
        }

        public async Task<AppUser> SetTwoFactorEnabledAsync(AppUser user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(user).Entity);
        }

        public async Task<AppUser> UpdateAsync(AppUser user)
        {
            var check = await userValidator.ValidateAsync(this, user);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            var model = _context.AppUser.Where(x => x.Id == user.Id).Single();

            /*
             * only persist certain fields.
             */

            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.LockoutEnabled = user.LockoutEnabled;
            model.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
            model.LastUpdated = DateTime.Now;
            model.Immutable = user.Immutable;

            _context.Entry(model).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(model).Entity);
        }

        protected async Task<IdentityResult> UpdatePassword(AppUser user, string newPassword)
        {
            if (_pv == null)
                throw new NotSupportedException();

            var result = await _pv.ValidateAsync(this, user, newPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            if (passwordHasher == null)
                throw new NotSupportedException();

            var hash = passwordHasher.HashPassword(user, newPassword);

            await SetPasswordHashAsync(user, hash);
            await SetSecurityStampAsync(user, RandomValues.CreateBase64String(32));

            return IdentityResult.Success;
        }

        protected async Task<PasswordVerificationResult> VerifyPasswordAsync(AppUser user, string password)
        {
            if (passwordHasher == null)
                throw new NotSupportedException();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return await Task.FromResult(PasswordVerificationResult.Failed);
        }
    }
}