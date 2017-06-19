using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    public partial class CustomUserManager : UserManager<AppUser, Guid>
    {
        private ConfigManager _config;
        public CustomUserStore LocalStore;
        public DataProtectorTokenProvider<AppUser, Guid> DataProtectorTokenProvider;
        public CustomPasswordValidator PasswordValidator;
        public CustomUserTokenProvider UserTokenProvider;

        public CustomUserManager(CustomUserStore store)
            : base(store)
        {
            if (store == null)
                throw new ArgumentNullException();

            _config = new ConfigManager();
            LocalStore = store;

            string[] uses = { Statics.ApiTokenConfirmEmail, Statics.ApiTokenResetPassword, Statics.ApiTokenConfirmPhone, Statics.ApiTokenConfirmTwoFactor };
            
            //create token protection provider...
            DataProtectorTokenProvider =
                new DataProtectorTokenProvider<AppUser, Guid>(new CustomDataProtectionProvider().Create(uses))
                {
                    TokenLifespan = TimeSpan.FromMinutes(_config.Tweaks.DefaultAccessTokenLife)
                };

            //create password validator...
            PasswordValidator =
                new CustomPasswordValidator
                {
                    RequiredLength = _config.Tweaks.DefaultPasswordLength
                };

            //create user token provider...
            UserTokenProvider =
                new CustomUserTokenProvider
                {
                    OtpTokenSize = _config.Tweaks.DefaultAuhthorizationCodeLength,
                    OtpTokenTimespan = _config.Tweaks.DefaultAuhthorizationCodeLife
                };
            
            MaxFailedAccessAttemptsBeforeLockout = _config.Tweaks.DefaultFailedAccessAttempts;
            UserLockoutEnabledByDefault = true;
        }

        public override async Task<IdentityResult> AccessFailedAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            user.LastLoginFailure = DateTime.Now;
            user.AccessFailedCount++;

            return await UpdateAsync(user);
        }

        public override async Task<IdentityResult> AddClaimAsync(Guid userId, Claim claim)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.AddClaimAsync(user, claim);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddPasswordAsync(Guid userId, string password)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            var hash = await LocalStore.GetPasswordHashAsync(user);

            if (hash != null)
                throw new InvalidOperationException();

            var result = await UpdatePassword(this.LocalStore, user, password);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddRefreshTokenAsync(AppUserRefreshToken token)
        {
            var user = await FindByIdAsyncDeprecated(token.UserId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.AddRefreshTokenAsync(token);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddToProviderAsync(Guid userId, string provider)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.AddToProviderAsync(user, provider);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddToRoleAsync(Guid userId, string role)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.AddToRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddToRolesAsync(Guid userId, params string[] roles)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            foreach (string role in roles)
                await AddToRoleAsync(userId, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            else if (!await CheckPasswordAsync(user, currentPassword))
                throw new InvalidOperationException();

            else
                return await UpdatePassword(this.LocalStore, user, newPassword);
        }

        public override async Task<IdentityResult> ChangePhoneNumberAsync(Guid userId, string phoneNumber, string token)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmPhone, token))
                throw new InvalidOperationException();
            else
            {
                user.PhoneNumber = phoneNumber;
                user.PhoneNumberConfirmed = true;

                return await UpdateAsync(user);
            }
        }

        public override async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            var found = await FindByIdAsyncDeprecated(user.Id);

            if (found == null)
                throw new ArgumentNullException();

            return await VerifyPasswordAsync(this.LocalStore, found, password);
        }

        public override async Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmEmail, token))
                throw new InvalidOperationException();
            else
            {
                await LocalStore.SetEmailConfirmedAsync(user, true);

                return IdentityResult.Success;
            }
        }

        public async Task<IdentityResult> ConfirmPasswordAsync(Guid userId, string token)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmEmail, token))
                throw new InvalidOperationException();
            else
            {
                await LocalStore.SetPasswordConfirmedAsync(user, true);

                return IdentityResult.Success;
            }
        }

        public async Task<IdentityResult> ConfirmPhoneNumberAsync(Guid userId, string token)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmEmail, token))
                throw new InvalidOperationException();
            else
            {
                await LocalStore.SetPhoneNumberConfirmedAsync(user, true);

                return IdentityResult.Success;
            }
        }

        public async Task<IdentityResult> CreateAsync(UserModel.Model user)
        {
            var found = await FindByNameAsyncDeprecated(user.Email);

            if (found != null)
                throw new ArgumentNullException();

            await LocalStore.CreateAsync(user);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateAsync(UserModel.Model user, string password)
        {
            var found = await FindByNameAsyncDeprecated(user.Email);

            if (found != null)
                throw new ArgumentNullException();

            await LocalStore.CreateAsync(user);

            //TODO - delete this sillyness...
            var foundUser = FindByNameAsyncDeprecated(user.Email).Result;

            await UpdatePassword(LocalStore, foundUser, password);

            return IdentityResult.Success;
        }

        public override Task<ClaimsIdentity> CreateIdentityAsync(AppUser user, string authenticationType)
        {
            IList<Claim> claimCollection = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
            };

            var claimsIdentity = new ClaimsIdentity(claimCollection, authenticationType);

            return Task.Run(() => claimsIdentity);
        }

        //protected async Task<SecurityToken> CreateSecurityTokenAsync(Guid userId)
        //{
        //    return
        //        new SecurityToken(Encoding.Unicode.GetBytes(await GetSecurityStampAsync(userId)));
        //}

        public async Task<IdentityResult> DeleteAsync(Guid user)
        {
            var found = await FindByIdAsyncDeprecated(user);

            if (found == null)
                throw new ArgumentNullException();

            await LocalStore.DeleteAsync(user);

            return IdentityResult.Success;
        }

        public async Task<UserModel.Model> FindByIdAsync(Guid userId)
        {
            return await LocalStore.FindByIdAsync(userId);
        }

        [System.Obsolete]
        public async Task<AppUser> FindByIdAsyncDeprecated(Guid userId)
        {
            return await Task.FromResult(LocalStore.Users.Where(x => x.Id == userId).SingleOrDefault());
        }

        public async Task<UserModel.Model> FindByNameAsync(string userName)
        {
            return await LocalStore.FindByNameAsync(userName);
        }

        [System.Obsolete]
        public Task<AppUser> FindByNameAsyncDeprecated(string userName)
        {
            return Task.FromResult(LocalStore.Users.Where(x => x.UserName == userName).SingleOrDefault());
        }

        public async Task<AppUserRefreshToken> FindRefreshTokenAsync(string token)
        {
            return await LocalStore.FindRefreshTokenAsync(token);
        }

        public async Task<AppUserRefreshToken> FindRefreshTokenByIdAsync(Guid tokenId)
        {
            return await LocalStore.FindRefreshTokenByIdAsync(tokenId);
        }

        public override async Task<IList<Claim>> GetClaimsAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.GetClaimsAsync(user);
        }

        public override async Task<string> GetEmailAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.GetEmailAsync(user);
        }

        public Task<IList<UserModel.Model>> GetListAsync()
        {
            return LocalStore.GetAll();
        }

        public override async Task<string> GetPhoneNumberAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.GetPhoneNumberAsync(user);
        }

        [System.Obsolete]
        public async Task<IList<string>> GetProvidersAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.GetProvidersAsync(user);
        }

        public override async Task<IList<string>> GetRolesAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.GetRolesAsync(user);
        }

        public override async Task<bool> GetTwoFactorEnabledAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.GetTwoFactorEnabledAsync(user);
        }

        public async override Task<string> GenerateChangePhoneNumberTokenAsync(Guid userId, string phoneNumber)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenConfirmPhone, userId);
        }

        public override async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenConfirmEmail, userId);
        }

        public override async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenResetPassword, userId);
        }

        public override async Task<string> GenerateTwoFactorTokenAsync(Guid userId, string twoFactorProvider)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenConfirmTwoFactor, userId);
        }

        public override async Task<string> GenerateUserTokenAsync(string purpose, Guid userId)
        {
            if (UserTokenProvider == null)
                throw new NotSupportedException();

            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await UserTokenProvider.GenerateAsync(purpose, this, user);
        }

        [System.Obsolete]
        private string GenerateSecurityStamp()
        {
            return Helper.EntrophyHelper.GenerateRandomBase64(32);
        }

        public override async Task<string> GetSecurityStampAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.GetSecurityStampAsync(user);
        }

        public override async Task<bool> HasPasswordAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.HasPasswordAsync(user);
        }

        [System.Obsolete]
        public async Task<bool> IsInProviderAsync(Guid userId, string role)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.IsInProviderAsync(user, role);
        }

        public override async Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await LocalStore.IsInRoleAsync(user, role);
        }

        public override async Task<bool> IsLockedOutAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (user.LockoutEnabled)
            {
                if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc <= DateTime.UtcNow)
                {
                    user.LockoutEnabled = false;
                    user.LockoutEndDateUtc = null;
                    await UpdateAsync(user);

                    return false;
                }
                else
                    return true;
            }
            else
            {
                user.LockoutEndDateUtc = null;
                await UpdateAsync(user);

                return false;
            }
        }

        public override async Task<IdentityResult> RemoveClaimAsync(Guid userId, Claim claim)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.RemoveClaimAsync(user, claim);

            return IdentityResult.Success;
        }

        [System.Obsolete]
        public async Task<IdentityResult> RemoveFromProviderAsync(Guid userId, string provider)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.RemoveFromProviderAsync(user, provider);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(Guid userId, string role)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.RemoveFromRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemoveFromRolesAsync(Guid userId, params string[] roles)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            foreach (string role in roles)
                await RemoveFromRoleAsync(userId, role);

            return IdentityResult.Success;
        }

        [System.Obsolete]
        public async Task<IdentityResult> RemoveRefreshTokenByIdAsync(Guid tokenId)
        {
            await LocalStore.RemoveRefreshTokenByIdAsync(tokenId);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ResetAccessFailedCountAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.ResetAccessFailedCountAsync(user);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemovePasswordAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await HasPasswordAsync(userId))
                throw new InvalidOperationException();

            await LocalStore.SetPasswordHashAsync(user, null);
            await LocalStore.SetSecurityStampAsync(user, null);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenResetPassword, token))
                throw new InvalidOperationException();

            var result = await UpdatePassword(this.LocalStore, user, newPassword);

            if (result.Succeeded)
                return await UpdateAsync(user);
            else
                return result;
        }

        public async Task<IdentityResult> SetEmailConfirmedAsync(Guid userId, bool confirmed)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.SetEmailConfirmedAsync(user, confirmed);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetPasswordConfirmedAsync(Guid userId, bool confirmed)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.SetPasswordConfirmedAsync(user, confirmed);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetPhoneNumberConfirmedAsync(Guid userId, bool confirmed)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.SetPhoneNumberConfirmedAsync(user, confirmed);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> SetTwoFactorEnabledAsync(Guid userId, bool enabled)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.SetTwoFactorEnabledAsync(user, enabled);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateAsync(AppUser user)
        {
            var found = await FindByIdAsyncDeprecated(user.Id);

            if (found == null)
                throw new ArgumentNullException();

            var result = await LocalStore.UserValidator.ValidateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            await LocalStore.UpdateAsync(user);

            return IdentityResult.Success;
        }

        protected override async Task<IdentityResult> UpdatePassword(IUserPasswordStore<AppUser, Guid> passwordStore, AppUser user, string newPassword)
        {
            if (PasswordValidator == null)
                throw new NotSupportedException();

            if (PasswordHasher == null)
                throw new NotSupportedException();

            var result = PasswordValidator.ValidateAsync(newPassword).Result;

            if (!result.Succeeded)
                throw new InvalidOperationException();

            var password = PasswordHasher.HashPassword(newPassword);

            await passwordStore.SetPasswordHashAsync(user, password);
            await UpdateSecurityStampAsync(user.Id);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateSecurityStampAsync(Guid userId)
        {
            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            await LocalStore.SetSecurityStampAsync(user, GenerateSecurityStamp());

            return IdentityResult.Success;
        }

        public override async Task<bool> VerifyChangePhoneNumberTokenAsync(Guid userId, string token, string phoneNumber)
        {
            if (UserTokenProvider == null)
                throw new NotSupportedException();

            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await UserTokenProvider.ValidateAsync(Statics.ApiTokenConfirmPhone, token, this, user);
        }

        protected override async Task<bool> VerifyPasswordAsync(IUserPasswordStore<AppUser, Guid> store, AppUser user, string password)
        {
            if (PasswordHasher == null)
                throw new NotSupportedException();

            var hash = await store.GetPasswordHashAsync(user);

            if (PasswordHasher.VerifyHashedPassword(hash, password) != PasswordVerificationResult.Failed)
                return true;
            else
                return false;
        }

        public override async Task<bool> VerifyTwoFactorTokenAsync(Guid userId, string twoFactorProvider, string token)
        {            
            throw new NotImplementedException();
        }

        public override async Task<bool> VerifyUserTokenAsync(Guid userId, string purpose, string token)
        {
            if (UserTokenProvider == null)
                throw new NotSupportedException();

            var user = await FindByIdAsyncDeprecated(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await UserTokenProvider.ValidateAsync(purpose, token, this, user);
        }
    }
}