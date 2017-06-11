using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    public class CustomUserManager : UserManager<AppUser, Guid>
    {
        private CustomUserStore _store;

        public CustomUserManager(CustomUserStore store)
            : base(store)
        {
            if (store == null)
                throw new ArgumentNullException();

            _store = store;
        }

        public override Task<IdentityResult> AccessFailedAsync(Guid userId)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                result.LastLoginFailure = DateTime.Now;
                result.AccessFailedCount++;

                _store.UpdateAsync(result);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> AddClaimAsync(Guid userId, Claim claim)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                _store.AddClaimAsync(result, claim);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> AddPasswordAsync(Guid userId, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> AddRefreshTokenAsync(AppUserToken token)
        {
            if (_store.IsUserValid(token.UserId))
            {
                _store.AddRefreshTokenAsync(token);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> AddToProviderAsync(Guid userId, string provider)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                _store.AddToProviderAsync(result, provider);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> AddToRoleAsync(Guid userId, string role)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                _store.AddToRoleAsync(result, role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> AddToRolesAsync(Guid userId, params string[] roles)
        {
            if (_store.IsUserValid(userId))
            {
                foreach (string role in roles)
                    AddToRoleAsync(userId, role);

                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                var check = CheckPasswordAsync(result, currentPassword);

                if (check.Result)
                {
                    var hash = PasswordHasher.HashPassword(newPassword);
                    var entrophy = Helper.EntrophyHelper.GenerateRandomBase64(32);

                    _store.SetSecurityStampAsync(result, entrophy);
                    _store.SetPasswordHashAsync(result, hash);

                    return Task.FromResult(IdentityResult.Success);
                }
                else
                    return Task.FromResult(IdentityResult.Failed(Statics.MsgUserInvalidCurrentPassword));
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            if (_store.IsUserValid(user))
            {
                var hash = _store.GetPasswordHashAsync(user).Result;

                if (PasswordHasher.VerifyHashedPassword(hash, password) != PasswordVerificationResult.Failed)
                    return Task.FromResult(true);
                else
                    return Task.FromResult(false);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> CreateAsync(AppUser user)
        {
            if (!_store.IsUserValid(user))
            {
                _store.CreateAsync(user);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            if (!_store.IsUserValid(user))
            {
                var hash = PasswordHasher.HashPassword(password);
                var stamp = Helper.EntrophyHelper.GenerateRandomBase64(32);

                _store.CreateAsync(user);
                _store.SetSecurityStampAsync(user, stamp);
                _store.SetPasswordHashAsync(user, hash);

                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
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

        public override Task<IdentityResult> DeleteAsync(AppUser user)
        {
            if (_store.IsUserValid(user))
            {
                _store.DeleteAsync(user);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<AppUser> FindByEmailAsync(string email)
        {
            return Task.FromResult(_store.Users.Where(x => x.Email == email).SingleOrDefault());
        }

        public override Task<AppUser> FindByIdAsync(Guid userId)
        {
            return Task.FromResult(_store.Users.Where(x => x.Id == userId).SingleOrDefault());
        }

        public override Task<AppUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(_store.Users.Where(x => x.UserName == userName).SingleOrDefault());
        }

        public Task<AppUserToken> FindRefreshTokenAsync(string ticket)
        {
            return _store.FindRefreshTokenAsync(ticket);
        }

        public Task<AppUserToken> FindRefreshTokenByIdAsync(Guid tokenId)
        {
            return _store.FindRefreshTokenByIdAsync(tokenId);
        }

        public override Task<IList<Claim>> GetClaimsAsync(Guid userId)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
                return _store.GetClaimsAsync(result);
            else
                throw new ArgumentNullException();
        }

        public Task<IList<string>> GetProvidersAsync(Guid userId)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
                return _store.GetProvidersAsync(result);
            else
                throw new ArgumentNullException();
        }

        public override Task<IList<string>> GetRolesAsync(Guid userId)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
                return _store.GetRolesAsync(result);
            else
                throw new ArgumentNullException();
        }

        public override Task<bool> HasPasswordAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInProviderAsync(Guid userId, string role)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
                return _store.IsInProviderAsync(result, role);
            else
                throw new ArgumentNullException();
        }

        public override Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
                return _store.IsInRoleAsync(result, role);
            else
                throw new ArgumentNullException();
        }

        public override Task<bool> IsLockedOutAsync(Guid userId)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                if (result.LockoutEnabled)
                {
                    if (result.LockoutEndDateUtc.HasValue && result.LockoutEndDateUtc <= DateTime.UtcNow)
                    {
                        result.LockoutEnabled = false;
                        result.LockoutEndDateUtc = null;
                        _store.UpdateAsync(result);

                        return Task.FromResult(false);
                    }
                    else
                        return Task.FromResult(true);
                }
                else
                {
                    result.LockoutEndDateUtc = null;
                    _store.UpdateAsync(result);

                    return Task.FromResult(false);
                }
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> RemoveClaimAsync(Guid userId, Claim claim)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                _store.RemoveClaimAsync(result, claim);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> RemoveFromProviderAsync(Guid userId, string provider)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                _store.RemoveFromProviderAsync(result, provider);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> RemoveFromRoleAsync(Guid userId, string role)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                _store.RemoveFromRoleAsync(result, role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> RemoveFromRolesAsync(Guid userId, params string[] roles)
        {
            if (_store.IsUserValid(userId))
            {
                foreach (string role in roles)
                    RemoveFromRoleAsync(userId, role);

                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> RemoveRefreshTokenByIdAsync(Guid tokenId)
        {
            _store.RemoveRefreshTokenByIdAsync(tokenId);
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> ResetAccessFailedCountAsync(Guid userId)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                _store.ResetAccessFailedCountAsync(result);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> SetPasswordAsync(Guid userId, string password)
        {
            AppUser result;

            if (_store.IsUserValid(userId, out result))
            {
                var hash = PasswordHasher.HashPassword(password);
                var entrophy = Helper.EntrophyHelper.GenerateRandomBase64(32);

                _store.SetSecurityStampAsync(result, entrophy);
                _store.SetPasswordHashAsync(result, hash);

                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> UpdateAsync(AppUser user)
        {
            if (_store.IsUserValid(user))
            {
                _store.UpdateAsync(user);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }
    }
}