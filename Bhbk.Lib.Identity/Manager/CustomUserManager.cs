using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    public partial class CustomUserManager : UserManager<AppUser, Guid>
    {
        private ConfigManager _config;
        public CustomUserStore Store;
        public DataProtectorTokenProvider<AppUser, Guid> DataProtectorTokenProvider;
        public CustomPasswordValidator PasswordValidator;
        public CustomUserTokenProvider UserTokenProvider;

        public CustomUserManager(CustomUserStore store)
            : base(store)
        {
            if (store == null)
                throw new ArgumentNullException();

            _config = new ConfigManager();
            Store = store;

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
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            user.LastLoginFailure = DateTime.Now;
            user.AccessFailedCount++;

            return await UpdateAsync(user);
        }

        public override async Task<IdentityResult> AddClaimAsync(Guid userId, Claim claim)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.AddClaimAsync(user, claim);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddPasswordAsync(Guid userId, string password)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            var hash = await Store.GetPasswordHashAsync(user);

            if (hash != null)
                throw new InvalidOperationException();

            var result = await UpdatePassword(this.Store, user, password);

            if (!result.Succeeded)
                throw new InvalidOperationException();
            
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddRefreshTokenAsync(UserRefreshTokenModel token)
        {
            if (!Store.Exists(token.UserId))
                throw new ArgumentNullException();

            await Store.AddRefreshTokenAsync(Store.Mf.Devolve.DoIt(token));

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddToProviderAsync(Guid userId, string provider)
        {
            if (!Store.Exists(userId))
                throw new ArgumentNullException();

            await Store.AddToProviderAsync(userId, provider);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddToRoleAsync(Guid userId, string role)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.AddToRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> AddToRolesAsync(Guid userId, params string[] roles)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            foreach (string role in roles)
                await AddToRoleAsync(userId, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            else if (!await CheckPasswordAsync(user, currentPassword))
                throw new InvalidOperationException();

            return await UpdatePassword(this.Store, user, newPassword);
        }

        public override async Task<IdentityResult> ChangePhoneNumberAsync(Guid userId, string phoneNumber, string token)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmPhone, token))
                throw new InvalidOperationException();

            user.PhoneNumber = phoneNumber;
            user.PhoneNumberConfirmed = true;

            return await UpdateAsync(user);
        }

        public async Task<bool> CheckPasswordAsync(Guid userId, string password)
        {
            var user = await FindByIdAsync(userId);

            if (user == null)
                throw new ArgumentNullException();

            //TODO - clean this up
            var devolve = Store.Mf.Devolve.DoIt(user);

            return await VerifyPasswordAsync(this.Store, devolve, password);
        }

        public override async Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmEmail, token))
                throw new InvalidOperationException();

            await Store.SetEmailConfirmedAsync(user, true);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ConfirmPasswordAsync(Guid userId, string token)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmEmail, token))
                throw new InvalidOperationException();

            await Store.SetPasswordConfirmedAsync(user, true);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ConfirmPhoneNumberAsync(Guid userId, string token)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenConfirmEmail, token))
                throw new InvalidOperationException();

            await Store.SetPhoneNumberConfirmedAsync(user, true);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateAsync(UserModel user)
        {
            if (Store.Exists(user.Id))
                throw new InvalidOperationException();

            var model = Store.Mf.Devolve.DoIt(user);
            await Store.CreateAsync(model);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateAsync(UserModel user, string password)
        {
            if (Store.Exists(user.Id))
                throw new InvalidOperationException();

            var model = Store.Mf.Devolve.DoIt(user);
            await Store.CreateAsync(model);

            //TODO - delete this sillyness...
            var found = Store.FindByName(user.Email);

            await UpdatePassword(Store, found, password);

            return IdentityResult.Success;
        }

        public override async Task<ClaimsIdentity> CreateIdentityAsync(AppUser user, string authenticationType)
        {
            IList<Claim> claims = new List<Claim>();

            foreach (string role in await Store.GetRolesAsync(user))
                claims.Add(new Claim(ClaimTypes.Role, role));

            foreach (Claim claim in await Store.GetClaimsAsync(user))
                claims.Add(claim);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            var result = new ClaimsIdentity(claims, authenticationType);

            return await Task.Run(() => result);
        }

        //protected async Task<SecurityToken> CreateSecurityTokenAsync(Guid userId)
        //{
        //    return
        //        new SecurityToken(Encoding.Unicode.GetBytes(await GetSecurityStampAsync(userId)));
        //}

        public async Task<IdentityResult> DeleteAsync(Guid user)
        {
            var found = Store.FindById(user);

            if (found == null)
                throw new ArgumentNullException();

            await Store.DeleteAsync(user);

            return IdentityResult.Success;
        }

        public async Task<UserModel> FindByIdAsync(Guid userId)
        {
            var model = Store.FindById(userId);

            if (model == null)
                return null;

            return Store.Mf.Evolve.DoIt(model);
        }

        public async Task<UserModel> FindByNameAsync(string userName)
        {
            var model = Store.FindByName(userName);

            if (model == null)
                return null;

            return Store.Mf.Evolve.DoIt(model);
        }

        public async Task<AppUserRefreshToken> FindRefreshTokenAsync(string token)
        {
            return await Store.FindRefreshTokenAsync(token);
        }

        public async Task<AppUserRefreshToken> FindRefreshTokenByIdAsync(Guid tokenId)
        {
            return await Store.FindRefreshTokenByIdAsync(tokenId);
        }

        //public async Task<UserClaimModel> GetClaimAsync(Guid userId, Guid claimId)
        //{
        //    var user = Store.FindById(userId);

        //    if (user == null)
        //        throw new InvalidOperationException();

        //    var claim = Store.GetClaimAsync(userId, claimId);

        //    if (claim == null)
        //        throw new InvalidOperationException();

        //    return Store.Mf.Evolve.DoIt(claim);
        //}

        public override async Task<IList<Claim>> GetClaimsAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetClaimsAsync(user);
        }

        public override async Task<string> GetEmailAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetEmailAsync(user);
        }

        public async Task<IList<UserModel>> GetListAsync()
        {
            IList<UserModel> result = new List<UserModel>();
            var users = Store.GetAll();

            foreach (AppUser user in users)
                result.Add(Store.Mf.Evolve.DoIt(user));

            return result;
        }

        public override async Task<string> GetPhoneNumberAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetPhoneNumberAsync(user);
        }

        public async Task<IList<string>> GetProvidersAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetProvidersAsync(user);
        }

        public override async Task<IList<string>> GetRolesAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetRolesAsync(user);
        }

        public override async Task<bool> GetTwoFactorEnabledAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetTwoFactorEnabledAsync(user);
        }

        public override async Task<string> GenerateChangePhoneNumberTokenAsync(Guid userId, string phoneNumber)
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

            var user = Store.FindById(userId);

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
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetSecurityStampAsync(user);
        }

        public override async Task<bool> HasPasswordAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.HasPasswordAsync(user);
        }

        public async Task<bool> IsInProviderAsync(Guid userId, string role)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.IsInProviderAsync(user, role);
        }

        public override async Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.IsInRoleAsync(user, role);
        }

        public override async Task<bool> IsLockedOutAsync(Guid userId)
        {
            var user = Store.FindById(userId);

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

        public async Task<IdentityResult> RemoveClaimAsync(Guid userId, Claim claim)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.RemoveClaimAsync(user, claim);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveFromProviderAsync(Guid userId, string provider)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.RemoveFromProviderAsync(user, provider);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(Guid userId, string role)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.RemoveFromRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemoveFromRolesAsync(Guid userId, params string[] roles)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            foreach (string role in roles)
                await RemoveFromRoleAsync(userId, role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveRefreshTokenByIdAsync(Guid tokenId)
        {
            await Store.RemoveRefreshTokenByIdAsync(tokenId);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ResetAccessFailedCountAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.ResetAccessFailedCountAsync(user);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> RemovePasswordAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await HasPasswordAsync(userId))
                throw new InvalidOperationException();

            await Store.SetPasswordHashAsync(user, null);
            await Store.SetSecurityStampAsync(user, null);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            if (!await VerifyUserTokenAsync(userId, Statics.ApiTokenResetPassword, token))
                throw new InvalidOperationException();

            var result = await UpdatePassword(this.Store, user, newPassword);

            if (result.Succeeded)
                return await UpdateAsync(user);
            else
                return result;
        }

        public async Task<IdentityResult> SetEmailConfirmedAsync(Guid userId, bool confirmed)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.SetEmailConfirmedAsync(user, confirmed);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetPasswordConfirmedAsync(Guid userId, bool confirmed)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.SetPasswordConfirmedAsync(user, confirmed);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetPhoneNumberConfirmedAsync(Guid userId, bool confirmed)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.SetPhoneNumberConfirmedAsync(user, confirmed);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> SetTwoFactorEnabledAsync(Guid userId, bool enabled)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.SetTwoFactorEnabledAsync(user, enabled);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateAsync(AppUser user)
        {
            var found = Store.FindById(user.Id);

            if (found == null)
                throw new ArgumentNullException();

            var result = await Store.UserValidator.ValidateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            await Store.UpdateAsync(user);

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
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.SetSecurityStampAsync(user, GenerateSecurityStamp());

            return IdentityResult.Success;
        }

        public override async Task<bool> VerifyChangePhoneNumberTokenAsync(Guid userId, string token, string phoneNumber)
        {
            if (UserTokenProvider == null)
                throw new NotSupportedException();

            var user = Store.FindById(userId);

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

        public override async Task<bool> VerifyUserTokenAsync(Guid userId, string purpose, string token)
        {
            if (UserTokenProvider == null)
                throw new NotSupportedException();

            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await UserTokenProvider.ValidateAsync(purpose, token, this, user);
        }
    }
}