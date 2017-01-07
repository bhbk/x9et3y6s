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
            _store = store;
        }

        public override Task<IdentityResult> AccessFailedAsync(Guid userId)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
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

            if (IsValidUser(userId, out result))
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
            if (IsValidUser(token.UserId))
            {
                _store.AddRefreshTokenAsync(token);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> AddToRoleAsync(Guid userId, string role)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
            {
                _store.AddToRoleAsync(result, role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> AddToRolesAsync(Guid userId, params string[] roles)
        {
            if (IsValidUser(userId))
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

            if (IsValidUser(userId, out result))
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
            if (IsValidUser(user))
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
            if (!IsValidUser(user))
            {
                _store.CreateAsync(user);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            if (!IsValidUser(user))
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
            if (IsValidUser(user))
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

        public Task<AppUserToken> FindRefreshTokenAsync(string tokenId)
        {
            return _store.FindRefreshTokenAsync(tokenId);
        }

        public override Task<IList<Claim>> GetClaimsAsync(Guid userId)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
                return _store.GetClaimsAsync(result);
            else
                throw new ArgumentNullException();
        }

        public override Task<IList<string>> GetRolesAsync(Guid userId)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
                return _store.GetRolesAsync(result);
            else
                throw new ArgumentNullException();
        }

        public override Task<bool> HasPasswordAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
                return _store.IsInRoleAsync(result, role);
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> RemoveClaimAsync(Guid userId, Claim claim)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
            {
                _store.RemoveClaimAsync(result, claim);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> RemoveFromRoleAsync(Guid userId, string role)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
            {
                _store.RemoveFromRoleAsync(result, role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> RemoveFromRolesAsync(Guid userId, params string[] roles)
        {
            if (IsValidUser(userId))
            {
                foreach (string role in roles)
                    RemoveFromRoleAsync(userId, role);

                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> RemoveRefreshTokenAsync(string token)
        {
            _store.RemoveRefreshTokenAsync(token);
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> ResetAccessFailedCountAsync(Guid userId)
        {
            AppUser result;

            if (IsValidUser(userId, out result))
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

            if (IsValidUser(userId, out result))
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
            if (IsValidUser(user))
            {
                _store.UpdateAsync(user);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        private bool IsValidUser(AppUser user)
        {
            var result = _store.Users.Where(x => x.Id == user.Id || x.UserName == user.UserName).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        private bool IsValidUser(Guid user)
        {
            var result = _store.Users.Where(x => x.Id == user).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        private bool IsValidUser(Guid user, out AppUser result)
        {
            result = _store.Users.Where(x => x.Id == user).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }
    }
}