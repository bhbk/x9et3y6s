using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.userstorebase-8
//https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-custom-storage-providers

namespace Bhbk.Lib.Identity.Stores
{
    public partial class CustomUserStore : UserStore<AppUser, AppRole, AppDbContext, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppUserToken, AppRoleClaim>
    {
        private readonly AppDbContext _context;

        public CustomUserStore(AppDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public Task AddRefreshTokenAsync(AppUserRefresh refresh)
        {
            var clean = _context.AppUserRefresh.Where(x => x.UserId == refresh.UserId
                && x.IssuedUtc > DateTime.UtcNow
                && x.ExpiresUtc < DateTime.UtcNow);

            if (clean != null)
                _context.AppUserRefresh.RemoveRange(clean);

            _context.AppUserRefresh.Add(refresh);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task AddClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (Claim claim in claims)
            {
                var model = new AppUserClaim()
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    ClaimValueType = claim.ValueType
                };

                _context.AppUserClaim.Add(model);
            }

            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task AddLoginAsync(AppUser user, UserLoginInfo info, CancellationToken cancellationToken = default(CancellationToken))
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
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task AddToRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = _context.AppRole.Where(x => x.Name == normalizedRoleName).SingleOrDefault();

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
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            user.SecurityStamp = RandomValues.CreateBase64String(32);

            _context.AppUser.Add(user);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            _context.AppUser.Remove(user);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public bool Exists(Guid UserId)
        {
            return _context.AppUser.Any(x => x.Id == UserId);
        }

        public bool Exists(string UserEmail)
        {
            return _context.AppUser.Any(x => x.Email == UserEmail);
        }

        public override Task<AppUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_context.AppUser.Where(x => x.Id.ToString() == userId).SingleOrDefault());
        }

        public override Task<AppUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_context.AppUser.Where(x => x.Email == normalizedEmail).SingleOrDefault());
        }

        public Task<AppUserRefresh> FindRefreshTokenAsync(string ticket)
        {
            return Task.FromResult(_context.AppUserRefresh.Where(x => x.ProtectedTicket == ticket).SingleOrDefault());
        }

        public Task<AppUserRefresh> FindRefreshTokenByIdAsync(Guid tokenId)
        {
            return Task.FromResult(_context.AppUserRefresh.Where(x => x.Id == tokenId).SingleOrDefault());
        }

        public IQueryable<AppUserRefresh> FindRefreshTokensAsync()
        {
            return _context.AppUserRefresh.Where(x => x.ExpiresUtc == DateTime.UtcNow);
        }

        public IQueryable<AppUser> Get(Expression<Func<AppUser, bool>> predicates = null,
            Func<IQueryable<AppUser>, IQueryable<AppUser>> orderBy = null,
            Func<IQueryable<AppUser>, IIncludableQueryable<AppUser, object>> includes = null,
            bool tracking = true)
        {
            IQueryable<AppUser> query = _context.AppUser.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return orderBy(query);

            return query;
        }

        public override Task<IList<Claim>> GetClaimsAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            IList<Claim> result = new List<Claim>();
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

            return Task.FromResult(result);
        }

        public override Task<string> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_context.Users.Where(x => x.Id == user.Id).Single().PasswordHash);
        }

        public Task<IList<string>> GetAudiencesAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (IList<string>)_context.AppAudience
                .Join(_context.AppRole, x => x.Id, y => y.AudienceId, (audience1, role1) => new {
                    AudienceId = audience1.Id,
                    RoleId = role1.Id
                })
                .Join(_context.AppUserRole, x => x.RoleId, y => y.RoleId, (role2, user2) => new {
                    AudienceId = role2.AudienceId,
                    UserId = user2.UserId
                })
                .Where(x => x.UserId == user.Id)
                .Select(x => x.AudienceId.ToString().ToLower())
                .Distinct()
                .ToList();

            return Task.FromResult(result);
        }

        public Task<IList<string>> GetLoginsAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
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

            return Task.FromResult(result);
        }

        public Task<IList<string>> GetRefreshTokensAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (IList<string>)_context.AppUserRefresh
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Id.ToString().ToLower())
                .ToList();

            return Task.FromResult(result);
        }

        public override Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
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

            return Task.FromResult(result);
        }

        [System.Obsolete]
        public Task<IList<string>> GetRolesResultIdAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
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

            return Task.FromResult(result);
        }

        public override Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public override Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = _context.AppUser.Where(x => x.Id == user.Id).SingleOrDefault();

            if (result == null)
                return Task.FromResult(false);

            else if (string.IsNullOrEmpty(result.PasswordHash))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<bool> IsInLoginAsync(AppUser user, string loginName)
        {
            var login = _context.AppLogin.Where(x => x.LoginProvider == loginName).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            else if (_context.AppUserLogin.Any(x => x.UserId == user.Id && x.LoginId == login.Id))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public override Task<bool> IsInRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = _context.AppRole.Where(x => x.Name == normalizedRoleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else if (_context.AppUserRole.Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public override Task RemoveClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (Claim claim in claims)
            {
                var result = _context.AppUserClaim.Where(x => x.UserId == user.Id
                    && x.ClaimType == claim.Type
                    && x.ClaimValue == claim.Value).SingleOrDefault();

                if (result == null)
                    throw new ArgumentNullException();

                _context.AppUserClaim.Remove(result);
            }

            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task RemoveLoginAsync(AppUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var login = _context.AppUserLogin.Where(x => x.LoginProvider == loginProvider).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            var result = _context.AppUserLogin.Where(x => x.UserId == user.Id && x.LoginId == login.LoginId).Single();

            _context.AppUserLogin.Remove(result);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task RemoveFromRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = _context.Roles.Where(x => x.Name == normalizedRoleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            var result = _context.AppUserRole.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.AppUserRole.Remove(result);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task RemoveRefreshTokenAsync(AppUser user, AppUserRefresh refresh)
        {
            var token = _context.AppUserRefresh.Where(x => x.UserId == user.Id && x.Id == refresh.Id);

            if (token == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(token);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task RemoveRefreshTokensAsync(AppUser user)
        {
            var token = _context.AppUserRefresh.Where(x => x.UserId == user.Id);

            if (token == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(token);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        [Obsolete]
        public override Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.EmailConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task SetImmutableAsync(AppUser user, bool enabled)
        {
            user.Immutable = enabled;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        [Obsolete]
        public Task SetPasswordConfirmedAsync(AppUser user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        [Obsolete]
        public override Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetSecurityStampAsync(AppUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.TwoFactorEnabled = enabled;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
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
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}