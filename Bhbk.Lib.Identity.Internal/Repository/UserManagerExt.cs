using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1

namespace Bhbk.Lib.Identity.Repository
{
    public partial class UserManagerExt : UserManager<AppUser>
    {
        public readonly ClaimsProvider ClaimProvider;
        public readonly PasswordHasher PasswordHasher;
        public readonly PasswordValidator PasswordValidator;
        public readonly UserStoreExt Store;
        public readonly UserValidator UserValidator;

        public UserManagerExt(UserStoreExt store,
            IOptions<IdentityOptions> options = null,
            IPasswordHasher<AppUser> passwordHasher = null,
            IEnumerable<IUserValidator<AppUser>> userValidators = null,
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators = null,
            ILookupNormalizer normalizer = null,
            IdentityErrorDescriber errors = null,
            IServiceProvider services = null,
            ILogger<UserManager<AppUser>> logger = null)
            : base(store, options, passwordHasher, userValidators, passwordValidators, normalizer, errors, services, logger)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
            ClaimProvider = new ClaimsProvider(this);
            PasswordHasher = new PasswordHasher();
            PasswordValidator = new PasswordValidator();
            UserValidator = new UserValidator();
        }

        public override async Task<IdentityResult> AccessFailedAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            user.LastLoginFailure = DateTime.Now;
            user.AccessFailedCount++;

            return await UpdateAsync(user);
        }

        public async Task<IdentityResult> AccessSuccessAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            user.LastLoginSuccess = DateTime.Now;
            user.AccessSuccessCount++;

            return await UpdateAsync(user);
        }

        public override async Task<IdentityResult> AddClaimAsync(AppUser user, Claim claim)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            List<Claim> list = new List<Claim>();
            list.Add(claim);

            await Store.AddClaimsAsync(user, list);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddLoginAsync(AppUser user, UserLoginInfo login)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            await Store.AddLoginAsync(user, login);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddPasswordAsync(AppUser user, string password)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            var hash = await Store.GetPasswordHashAsync(user);

            if (hash != null)
                throw new InvalidOperationException();

            return await UpdatePassword(this.Store, user, password);
        }

        public async Task<IdentityResult> AddRefreshTokenAsync(AppUserRefresh refresh)
        {
            if (!Store.Exists(refresh.UserId))
                throw new ArgumentNullException();

            await Store.AddRefreshTokenAsync(refresh);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddToRoleAsync(AppUser user, string role)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            await Store.AddToRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddToRolesAsync(AppUser user, IEnumerable<string> roles)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            foreach (string role in roles)
                await AddToRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            else if (!await CheckPasswordAsync(user, currentPassword))
                throw new InvalidOperationException();

            return await UpdatePassword(this.Store, user, newPassword);
        }

        public override async Task<IdentityResult> ChangePhoneNumberAsync(AppUser user, string phoneNumber, string token)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(user, string.Empty, phoneNumber, token))
                throw new InvalidOperationException();

            user.PhoneNumber = phoneNumber;
            user.PhoneNumberConfirmed = true;

            return await UpdateAsync(user);
        }

        public override async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            if (await VerifyPasswordAsync(Store, user, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }

        public async Task<int> Count(Expression<Func<AppUser, bool>> predicates = null)
        {
            var query = Store.Context.AppUser.AsQueryable();

            if (predicates != null)
                return await query.Where(predicates).CountAsync();

            return await query.CountAsync();
        }

        public override async Task<IdentityResult> CreateAsync(AppUser user)
        {
            if (Store.Exists(user.Id))
                throw new InvalidOperationException();

            var check = await UserValidator.ValidateAsync(this, user);

            if (!check.Succeeded)
                return check;

            return await Store.CreateAsync(user);
        }

        public override async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            if (Store.Exists(user.Id))
                throw new InvalidOperationException();

            var check = await UserValidator.ValidateAsync(this, user);

            if (!check.Succeeded)
                return check;

            var create = await Store.CreateAsync(user);

            if (!create.Succeeded)
                return create;

            return await UpdatePassword(this.Store, user, password);
        }

        public override async Task<IdentityResult> DeleteAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.DeleteAsync(user, new CancellationToken());
        }

        public override async Task<AppUser> FindByIdAsync(string userId)
        {
            Guid result;

            if (!Guid.TryParse(userId, out result))
                throw new InvalidOperationException();

            return await Store.FindByIdAsync(userId);
        }

        public override async Task<AppUser> FindByEmailAsync(string email)
        {
            return await Store.FindByEmailAsync(email);
        }

        public async Task<AppUserRefresh> FindRefreshTokenAsync(string token)
        {
            return await Store.FindRefreshTokenAsync(token);
        }

        public async Task<AppUserRefresh> FindRefreshTokenByIdAsync(string tokenId)
        {
            Guid result;

            if (!Guid.TryParse(tokenId, out result))
                throw new InvalidOperationException();

            return await Store.FindRefreshTokenByIdAsync(result);
        }

        public async Task<IQueryable<AppUser>> GetAsync(Expression<Func<AppUser, bool>> predicates = null,
            Func<IQueryable<AppUser>, IQueryable<AppUser>> orderBy = null,
            Func<IQueryable<AppUser>, IIncludableQueryable<AppUser, object>> includes = null,
            bool tracking = true)
        {
            var query = Store.Context.AppUser.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            query.Include("AppUserRole.Role").Load();

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public override async Task<IList<Claim>> GetClaimsAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.GetClaimsAsync(user);
        }

        public async Task<IList<string>> GetClientsAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.GetClientsAsync(user);
        }

        public async Task<IList<string>> GetLoginsAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.GetLoginsAsync(user);
        }

        public async Task<IList<string>> GetRefreshTokensAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.GetRefreshTokensAsync(user);
        }

        public override async Task<IList<string>> GetRolesAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.GetRolesAsync(user);
        }

        [System.Obsolete]
        public async Task<IList<string>> GetRolesResultIdAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.GetRolesResultIdAsync(user);
        }

        public override async Task<bool> GetTwoFactorEnabledAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.GetTwoFactorEnabledAsync(user);
        }

        public override async Task<bool> HasPasswordAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.HasPasswordAsync(user);
        }

        public async Task<bool> IsInLoginAsync(AppUser user, string login)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.IsInLoginAsync(user, login);
        }

        public override async Task<bool> IsInRoleAsync(AppUser user, string role)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            return await Store.IsInRoleAsync(user, role);
        }

        public override async Task<bool> IsLockedOutAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

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

        public override async Task<IdentityResult> RemoveClaimAsync(AppUser user, Claim claim)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            List<Claim> list = new List<Claim>();
            list.Add(claim);

            await Store.RemoveClaimsAsync(user, list);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemoveLoginAsync(AppUser user, string loginProvider, string providerKey)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            await Store.RemoveLoginAsync(user, loginProvider, providerKey);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(AppUser user, string role)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            await Store.RemoveFromRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemoveFromRolesAsync(AppUser user, IEnumerable<string> roles)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            foreach (string role in roles)
                await RemoveFromRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveRefreshTokenAsync(AppUser user, AppUserRefresh refresh)
        {
            await Store.RemoveRefreshTokenAsync(user, refresh);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveRefreshTokensAsync(AppUser user)
        {
            await Store.RemoveRefreshTokensAsync(user);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemovePasswordAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            await Store.SetPasswordHashAsync(user, null);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> SetTwoFactorEnabledAsync(AppUser user, bool enabled)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            await Store.SetTwoFactorEnabledAsync(user, enabled);

            return IdentityResult.Success;
        }


        public override async Task<IdentityResult> UpdateAsync(AppUser user)
        {
            if (!Store.Exists(user.Id))
                throw new ArgumentNullException();

            var result = await UserValidator.ValidateAsync(this, user);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            await Store.UpdateAsync(user);

            return IdentityResult.Success;
        }

        protected async Task<IdentityResult> UpdatePassword(IUserPasswordStore<AppUser> passwordStore, AppUser user, string newPassword)
        {
            if (PasswordValidators == null)
                throw new NotSupportedException();

            if (PasswordHasher == null)
                throw new NotSupportedException();

            var result = await PasswordValidator.ValidateAsync(this, user, newPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            var hash = PasswordHasher.HashPassword(user, newPassword);

            await passwordStore.SetPasswordHashAsync(user, hash, new CancellationToken());
            await Store.SetSecurityStampAsync(user, RandomValues.CreateBase64String(32));

            return IdentityResult.Success;
        }

        protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<AppUser> store, AppUser user, string password)
        {
            if (PasswordHasher == null)
                throw new NotSupportedException();

            var hash = await Store.GetPasswordHashAsync(user, new CancellationToken());

            if (PasswordHasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return PasswordVerificationResult.Failed;
        }
    }
}