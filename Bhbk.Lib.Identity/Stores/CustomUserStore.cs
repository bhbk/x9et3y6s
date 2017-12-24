using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private AppDbContext _ioc;

        public CustomUserStore(AppDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
            if (context == null)
                throw new ArgumentNullException();

            _ioc = context;
        }

        public Task AddRefreshTokenAsync(AppUserRefresh refresh)
        {
            var clean = _ioc.AppUserRefresh.Where(x => x.UserId == refresh.UserId
                && x.IssuedUtc > DateTime.UtcNow
                && x.ExpiresUtc < DateTime.UtcNow);

            if (clean != null)
                _ioc.AppUserRefresh.RemoveRange(clean);

            _ioc.AppUserRefresh.Add(refresh);
            _ioc.SaveChanges();

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

                _ioc.AppUserClaim.Add(model);
            }

            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task AddLoginAsync(AppUser user, UserLoginInfo info, CancellationToken cancellationToken = default(CancellationToken))
        {
            var login = _ioc.AppLogin.Where(x => x.LoginProvider == info.LoginProvider).SingleOrDefault();

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

            _ioc.AppUserLogin.Add(result);
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task AddToRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = _ioc.AppRole.Where(x => x.Name == normalizedRoleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            AppUserRole result = new AppUserRole()
            {
                UserId = user.Id,
                RoleId = role.Id,
                Created = DateTime.Now,
                Immutable = false
            };

            _ioc.AppUserRole.Add(result);
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            _ioc.AppUser.Add(user);
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            _ioc.AppUser.Remove(user);
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public bool Exists(Guid UserId)
        {
            return _ioc.AppUser.Any(x => x.Id == UserId);
        }

        public bool Exists(string UserEmail)
        {
            return _ioc.AppUser.Any(x => x.Email == UserEmail);
        }

        public override Task<AppUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_ioc.AppUser.Where(x => x.Id.ToString() == userId).SingleOrDefault());
        }

        public override Task<AppUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_ioc.AppUser.Where(x => x.Email == normalizedUserName).SingleOrDefault());
        }

        public Task<AppUserRefresh> FindRefreshTokenAsync(string ticket)
        {
            return Task.FromResult(_ioc.AppUserRefresh.Where(x => x.ProtectedTicket == ticket).SingleOrDefault());
        }

        public Task<AppUserRefresh> FindRefreshTokenByIdAsync(Guid tokenId)
        {
            return Task.FromResult(_ioc.AppUserRefresh.Where(x => x.Id == tokenId).SingleOrDefault());
        }

        public IEnumerable<AppUserRefresh> FindRefreshTokensAsync()
        {
            return _ioc.AppUserRefresh.Where(x => x.ExpiresUtc == DateTime.UtcNow);
        }

        public IEnumerable<AppUser> Get(Expression<Func<AppUser, bool>> filter = null,
            Func<IQueryable<AppUser>, IOrderedQueryable<AppUser>> orderBy = null, string includes = "")
        {
            IQueryable<AppUser> query = _ioc.AppUser.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public IList<AppUser> Get()
        {
            return _ioc.AppUser.ToList();
        }

        public override Task<IList<Claim>> GetClaimsAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            IList<Claim> result = new List<Claim>();
            var map = _ioc.AppUserClaim.Where(x => x.UserId == user.Id).ToList();

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
            return Task.FromResult(_ioc.Users.Where(x => x.Id == user.Id).Single().PasswordHash);
        }

        public override Task<string> GetEmailAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.Email);
        }

        public override Task<string> GetPhoneNumberAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<IList<string>> GetAudiencesAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (IList<string>)_ioc.AppAudience
                .Join(_ioc.AppRole, x => x.Id, y => y.AudienceId, (audience, role) => new {
                    AudienceId = audience.Id,
                    RoleId = role.Id
                })
                .Join(_ioc.AppUserRole, x => x.RoleId, y => y.RoleId, (trole, tuser) => new {
                    AudienceId = trole.AudienceId,
                    UserId = tuser.UserId
                })
                .Where(x => x.UserId == user.Id)
                .Select(x => x.AudienceId.ToString())
                .Distinct()
                .ToList();

            return Task.FromResult(result);
        }

        public Task<IList<string>> GetLoginsAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (IList<string>)_ioc.AppLogin
                .Join(_ioc.AppUserLogin, x => x.Id, y => y.LoginId, (tlogin, tuser) => new {
                    LoginId = tlogin.Id,
                    UserId = tuser.UserId
                })
                .Where(x => x.UserId == user.Id)
                .Select(x => x.LoginId.ToString())
                .Distinct()
                .ToList();

            return Task.FromResult(result);
        }

        public override Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (IList<string>)_ioc.AppRole
                .Join(_ioc.AppUserRole, x => x.Id, y => y.RoleId, (trole, tuser) => new {
                    RoleId = trole.Id,
                    UserId = tuser.UserId
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
            var result = _ioc.Users.Where(x => x.Id == user.Id).SingleOrDefault();

            if (result == null)
                return Task.FromResult(false);
            else
            {
                if (string.IsNullOrEmpty(result.PasswordHash))
                    return Task.FromResult(false);

                return Task.FromResult(true);
            }
        }

        public Task<bool> IsInLoginAsync(AppUser user, string loginName)
        {
            var login = _ioc.AppLogin.Where(x => x.LoginProvider == loginName).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            if (_ioc.AppUserLogin.Any(x => x.UserId == user.Id && x.LoginId == login.Id))
                return Task.FromResult(true);

            else
                return Task.FromResult(false);
        }

        public override Task<bool> IsInRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = _ioc.AppRole.Where(x => x.Name == normalizedRoleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else if (_ioc.AppUserRole.Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                return Task.FromResult(true);

            else
                return Task.FromResult(false);
        }

        public override Task RemoveClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (Claim claim in claims)
            {
                var result = _ioc.AppUserClaim.Where(x => x.UserId == user.Id
                    && x.ClaimType == claim.Type
                    && x.ClaimValue == claim.Value).SingleOrDefault();

                if (result == null)
                    throw new ArgumentNullException();

                _ioc.AppUserClaim.Remove(result);
            }

            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task RemoveLoginAsync(AppUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var login = _ioc.AppUserLogin.Where(x => x.LoginProvider == loginProvider).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            var result = _ioc.AppUserLogin.Where(x => x.UserId == user.Id && x.LoginId == login.LoginId).Single();

            _ioc.AppUserLogin.Remove(result);
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task RemoveFromRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = _ioc.Roles.Where(x => x.Name == normalizedRoleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            var result = _ioc.AppUserRole.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _ioc.AppUserRole.Remove(result);
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task RemoveRefreshTokenAsync(AppUser user, AppUserRefresh refresh)
        {
            var token = _ioc.AppUserRefresh.Where(x => x.UserId == user.Id && x.Id == refresh.Id);

            if (token == null)
                throw new ArgumentNullException();

            else
            {
                _ioc.AppUserRefresh.RemoveRange(token);
                _ioc.SaveChanges();
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task RemoveRefreshTokensAsync(AppUser user)
        {
            var token = _ioc.AppUserRefresh.Where(x => x.UserId == user.Id);

            if (token == null)
                throw new ArgumentNullException();

            else
            {
                _ioc.AppUserRefresh.RemoveRange(token);
                _ioc.SaveChanges();
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task ResetAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.LastLoginSuccess = DateTime.Now;
            user.AccessSuccessCount++;
            user.AccessFailedCount = 0;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.EmailConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task SetImmutableEnabledAsync(AppUser user, bool enabled)
        {
            user.Immutable = enabled;
            user.LastUpdated = DateTime.Now;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task SetPasswordConfirmedAsync(AppUser user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = DateTime.Now;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetSecurityStampAsync(AppUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = DateTime.Now;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.TwoFactorEnabled = enabled;
            user.LastUpdated = DateTime.Now;

            _ioc.Entry(user).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var model = _ioc.AppUser.Where(x => x.Id == user.Id).Single();

            model.UserName = user.Email;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.LockoutEnabled = user.LockoutEnabled;
            model.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
            model.Immutable = user.Immutable;
            model.LastUpdated = DateTime.Now;

            _ioc.Entry(model).State = EntityState.Modified;
            _ioc.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}