using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Managers
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1?view=aspnetcore-2.0
    public partial class CustomUserManager : UserManager<AppUser>
    {
        private ConfigManager _config;
        public CustomUserStore Store;
        public DataProtectorTokenProvider<AppUser> DataProtectorTokenProvider;
        public IPasswordHasher<AppUser> PasswordHasher;
        public IPasswordValidator<AppUser> PasswordValidator;
        public IUserValidator<AppUser> UserValidator;
        public CustomUserTokenProvider UserTokenProvider;

        public CustomUserManager(CustomUserStore store,
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

            _config = new ConfigManager();

            Store = store;
            PasswordHasher = new CustomPasswordHasher();
            PasswordValidator = new CustomPasswordValidator();
            UserValidator = new CustomUserValidator();

            string[] uses = { Statics.ApiTokenConfirmEmail, Statics.ApiTokenResetPassword, Statics.ApiTokenConfirmPhone, Statics.ApiTokenConfirmTwoFactor };

            //create token protection provider...
            DataProtectionTokenProviderOptions tokenProviderOptions = new DataProtectionTokenProviderOptions()
            {
                TokenLifespan = TimeSpan.FromMinutes(_config.Tweaks.DefaultAccessTokenLife)
            };
            //DataProtectorTokenProvider =
            //    new DataProtectorTokenProvider<AppUser>(new CustomDataProtectionProvider().Create(uses), tokenProviderOptions)
            //    {
            //        //TokenLifespan = TimeSpan.FromMinutes(_config.Tweaks.DefaultAccessTokenLife)
            //    };

            //create user token provider...
            UserTokenProvider =
                new CustomUserTokenProvider
                {
                    OtpTokenSize = _config.Tweaks.DefaultAuhthorizationCodeLength,
                    OtpTokenTimespan = _config.Tweaks.DefaultAuhthorizationCodeLife
                };
        }

        public async Task<IdentityResult> AccessFailedAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            user.LastLoginFailure = DateTime.Now;
            user.AccessFailedCount++;

            return await UpdateAsync(user);
        }

        public async Task<IdentityResult> AddClaimAsync(Guid userId, Claim claim)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            List<Claim> list = new List<Claim>();
            list.Add(claim);

            await Store.AddClaimsAsync(user, list);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddLoginAsync(Guid userId, UserLoginModel login)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            var info = new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName);

            await Store.AddLoginAsync(user, info);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddPasswordAsync(Guid userId, string password)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            var hash = await Store.GetPasswordHashAsync(user);

            if (hash != null)
                throw new InvalidOperationException();

            var result = await UpdatePassword(Store, user, password);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddRefreshTokenAsync(UserRefreshModel token)
        {
            if (!Store.Exists(token.UserId))
                throw new ArgumentNullException();

            await Store.AddRefreshTokenAsync(Store.Mf.Devolve.DoIt(token));

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddToRoleAsync(Guid userId, string roleName)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.AddToRoleAsync(user, roleName);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddToRolesAsync(Guid userId, params string[] roles)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            foreach (string role in roles)
                await AddToRoleAsync(userId, role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            else if (!await CheckPasswordAsync(user, currentPassword))
                throw new InvalidOperationException();

            return await UpdatePassword(Store, user, newPassword);
        }

        public async Task<IdentityResult> ChangePhoneNumberAsync(Guid userId, string phoneNumber, string token)
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

            if (await VerifyPasswordAsync(Store, devolve, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token)
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

        public async Task<ClaimsIdentity> CreateIdentityAsync(AppUser user, string authenticationType)
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
        
        public async Task<IdentityResult> DeleteAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.DeleteAsync(user, new CancellationToken());

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

        public async Task<AppUserRefresh> FindRefreshTokenAsync(string token)
        {
            return await Store.FindRefreshTokenAsync(token);
        }

        public async Task<AppUserRefresh> FindRefreshTokenByIdAsync(Guid tokenId)
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

        public async Task<IList<Claim>> GetClaimsAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetClaimsAsync(user);
        }

        public async Task<string> GetEmailAsync(Guid userId)
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

        public async Task<string> GetPhoneNumberAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetPhoneNumberAsync(user);
        }

        public async Task<IList<string>> GetLoginsAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetLoginsAsync(user);
        }

        public async Task<IList<string>> GetRolesAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetRolesAsync(user);
        }

        public async Task<bool> GetTwoFactorEnabledAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetTwoFactorEnabledAsync(user);
        }

        public async Task<string> GenerateChangePhoneNumberTokenAsync(Guid userId, string phoneNumber)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenConfirmPhone, userId);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenConfirmEmail, userId);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenResetPassword, userId);
        }

        public async Task<string> GenerateTwoFactorTokenAsync(Guid userId, string twoFactorProvider)
        {
            return await GenerateUserTokenAsync(Statics.ApiTokenConfirmTwoFactor, userId);
        }

        public async Task<string> GenerateUserTokenAsync(string purpose, Guid userId)
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
            return Helpers.EntrophyHelper.GenerateRandomBase64(32);
        }

        public async Task<string> GetSecurityStampAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.GetSecurityStampAsync(user);
        }

        public async Task<bool> HasPasswordAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.HasPasswordAsync(user);
        }

        public async Task<bool> IsInLoginAsync(Guid userId, string role)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.IsInLoginAsync(user, role);
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await Store.IsInRoleAsync(user, role);
        }

        public async Task<bool> IsLockedOutAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
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

        public async Task<IdentityResult> RemoveClaimAsync(Guid userId, Claim claim)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            List<Claim> list = new List<Claim>();
            list.Add(claim);

            await Store.RemoveClaimsAsync(user, list);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveFromProviderAsync(Guid userId, string loginProvider)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.RemoveFromLoginAsync(user, loginProvider);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(Guid userId, string role)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.RemoveFromRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(Guid userId, params string[] roles)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            foreach (string role in roles)
                await RemoveFromRoleAsync(userId, role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveRefreshTokenAsync(Guid clientId, Guid audienceId, Guid userId)
        {
            await Store.RemoveRefreshTokenAsync(clientId, audienceId, userId);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ResetAccessFailedCountAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.ResetAccessFailedCountAsync(user);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemovePasswordAsync(Guid userId)
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

        public async Task<IdentityResult> ResetPasswordAsync(Guid userId, string token, string newPassword)
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

        public async Task<IdentityResult> SetTwoFactorEnabledAsync(Guid userId, bool enabled)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.SetTwoFactorEnabledAsync(user, enabled);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(AppUser user)
        {
            var found = Store.FindById(user.Id);

            if (found == null)
                throw new ArgumentNullException();

            //Don't forget to update this
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

            var result = PasswordValidator.ValidateAsync(this, user, newPassword).Result;

            if (!result.Succeeded)
                throw new InvalidOperationException();

            var password = PasswordHasher.HashPassword(user, newPassword);

            await passwordStore.SetPasswordHashAsync(user, password, new CancellationToken());
            await UpdateSecurityStampAsync(user.Id);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateSecurityStampAsync(Guid userId)
        {
            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            await Store.SetSecurityStampAsync(user, GenerateSecurityStamp());

            return IdentityResult.Success;
        }

        public async Task<bool> VerifyChangePhoneNumberTokenAsync(Guid userId, string token, string phoneNumber)
        {
            if (UserTokenProvider == null)
                throw new NotSupportedException();

            var user = Store.FindById(userId);

            if (user == null)
                throw new ArgumentNullException();

            return await UserTokenProvider.ValidateAsync(Statics.ApiTokenConfirmPhone, token, this, user);
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

        public async Task<bool> VerifyUserTokenAsync(Guid userId, string purpose, string token)
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