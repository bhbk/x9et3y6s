using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    //https://msdn.microsoft.com/en-us/library/dn613259(v=vs.108).aspx
    public class CustomUserStore : UserStore<AppUser, AppRole, Guid, AppUserProvider, AppUserRole, AppUserClaim>
    {
        private CustomIdentityDbContext _context;

        public CustomUserStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public override Task AddClaimAsync(AppUser user, Claim claim)
        {
            var newClaim = new AppUserClaim()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            };

            _context.AppUserClaim.Add(newClaim);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task AddRefreshTokenAsync(AppUserToken token)
        {
            var result = _context.AppUserToken.Where(x => x.UserId == token.UserId && x.AudienceId == token.AudienceId).SingleOrDefault();

            if (result != null)
                _context.AppUserToken.Remove(result);

            _context.AppUserToken.Add(token);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task AddToProviderAsync(AppUser user, string providerName)
        {
            var provider = _context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault();

            if (provider == null)
                throw new ArgumentNullException();

            else
            {
                AppUserProvider result = new AppUserProvider()
                {
                    ProviderId = provider.Id,
                    UserId = user.Id,
                    Enabled = true,
                    Created = DateTime.Now,
                    Immutable = false
                };

                _context.AppUserProvider.Add(result);
                _context.SaveChanges();

                return Task.FromResult(IdentityResult.Success);
            }
        }

        public override Task AddToRoleAsync(AppUser user, string roleName)
        {
            var role = _context.Roles.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else
            {
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
        }

        public override Task CreateAsync(AppUser user)
        {
            user.UserName = user.Email;
            user.Created = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task DeleteAsync(AppUser user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<AppUserToken> FindRefreshTokenAsync(string tokenId)
        {
            return Task.FromResult(_context.AppUserToken.Where(x => x.Id.ToString() == tokenId).SingleOrDefault());
        }

        public override Task<IList<Claim>> GetClaimsAsync(AppUser user)
        {
            IList<Claim> result = new List<Claim>();
            var claims = _context.AppUserClaim.Where(x => x.UserId == user.Id).ToList();

            if (claims == null)
                throw new InvalidOperationException();

            else
            {
                foreach (var claim in claims)
                {
                    var model = new Claim(claim.ClaimType,
                        claim.ClaimValue,
                        claim.ClaimValueType,
                        claim.Issuer,
                        claim.OriginalIssuer,
                        new ClaimsIdentity(claim.Subject));

                    result.Add(model);
                }

                return Task.FromResult(result);
            }
        }

        public override Task<string> GetPasswordHashAsync(AppUser user)
        {
            return Task.FromResult(_context.Users.Where(x => x.Id == user.Id).Single().PasswordHash);
        }

        public Task<IList<string>> GetProvidersAsync(AppUser user)
        {
            IList<string> result = new List<string>();
            var providers = _context.AppUserProvider.Where(x => x.UserId == user.Id).ToList();

            if (providers == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppUserProvider provider in providers)
                    result.Add(_context.AppProvider.Where(x => x.Id == provider.ProviderId).Select(r => r.Name).Single());

                return Task.FromResult(result);
            }
        }

        public override Task<IList<string>> GetRolesAsync(AppUser user)
        {
            IList<string> result = new List<string>();
            var roles = _context.AppUserRole.Where(x => x.UserId == user.Id).ToList();

            if (roles == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppUserRole role in roles)
                    result.Add(_context.Roles.Where(x => x.Id == role.RoleId).Select(r => r.Name).Single());

                return Task.FromResult(result);
            }
        }

        public override Task<bool> HasPasswordAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public override Task<AppUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInProviderAsync(AppUser user, string providerName)
        {
            var provider = _context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault();

            if (provider == null)
                throw new ArgumentNullException();

            else if (_context.AppUserProvider.Any(x => x.UserId == user.Id && x.ProviderId == provider.Id))
                return Task.FromResult(true);

            else
                return Task.FromResult(false);
        }

        public override Task<bool> IsInRoleAsync(AppUser user, string roleName)
        {
            var role = _context.Roles.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else if (_context.AppUserRole.Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                return Task.FromResult(true);

            else
                return Task.FromResult(false);
        }

        public bool IsValidUser(AppUser user)
        {
            var result = _context.Users.Where(x => x.Id == user.Id || x.UserName == user.UserName).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public bool IsValidUser(Guid user)
        {
            var result = _context.Users.Where(x => x.Id == user).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public bool IsValidUser(Guid user, out AppUser result)
        {
            result = _context.Users.Where(x => x.Id == user).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public override Task RemoveClaimAsync(AppUser user, Claim claim)
        {
            var result = _context.AppUserClaim.Where(x => x.UserId == user.Id
                && x.ClaimType == claim.Type
                && x.ClaimValue == claim.Value).SingleOrDefault();

            if (result == null)
                throw new ArgumentNullException();

            else
            {
                _context.AppUserClaim.Remove(result);
                _context.SaveChanges();

                return Task.FromResult(IdentityResult.Success);
            }
        }

        public Task RemoveFromProviderAsync(AppUser user, string providerName)
        {
            var provider = _context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault();

            if (provider == null)
                throw new ArgumentNullException();

            else
            {
                var result = _context.AppUserProvider.Where(x => x.UserId == user.Id && x.ProviderId == provider.Id).Single();

                _context.AppUserProvider.Remove(result);
                _context.SaveChanges();

                return Task.FromResult(IdentityResult.Success);
            }
        }

        public override Task RemoveFromRoleAsync(AppUser user, string roleName)
        {
            var role = _context.Roles.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else
            {
                var result = _context.AppUserRole.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

                _context.AppUserRole.Remove(result);
                _context.SaveChanges();

                return Task.FromResult(IdentityResult.Success);
            }
        }

        public Task RemoveRefreshTokenAsync(string tokenId)
        {
            var result = _context.AppUserToken.Where(x => x.Id.ToString() == tokenId).SingleOrDefault();

            if (result != null)
            {
                _context.AppUserToken.Remove(result);
                _context.SaveChanges();
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task ResetAccessFailedCountAsync(AppUser user)
        {
            user.LastLoginSuccess = DateTime.Now;
            user.AccessSuccessCount++;
            user.AccessFailedCount = 0;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetPasswordHashAsync(AppUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetSecurityStampAsync(AppUser user, string stamp)
        {
            user.SecurityStamp = stamp;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task UpdateAsync(AppUser user)
        {
            user.UserName = user.Email;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}