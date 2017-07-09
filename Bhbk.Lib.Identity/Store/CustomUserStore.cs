using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    //https://msdn.microsoft.com/en-us/library/dn613259(v=vs.108).aspx
    public partial class CustomUserStore : UserStore<AppUser, AppRole, Guid, AppUserProvider, AppUserRole, AppUserClaim>
    {
        private CustomIdentityDbContext _context;
        private CustomIdentityValidator _validator;
        private DbSet<AppUser> _data;
        public ModelFactory Mf;

        public CustomUserStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _data = _context.Set<AppUser>();
            _validator = new CustomIdentityValidator(_context);
            Mf = new ModelFactory(_context);
        }

        public IIdentityValidator<AppUser> UserValidator
        {
            get
            {
                return _validator;
            }
        }

        public override Task AddClaimAsync(AppUser user, Claim claim)
        {
            var model = new AppUserClaim()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            };

            _context.AppUserClaim.Add(model);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task AddToProviderAsync(Guid userId, string providerName)
        {
            var provider = _context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault();

            if (provider == null)
                throw new ArgumentNullException();

            AppUserProvider result = new AppUserProvider()
            {
                ProviderId = provider.Id,
                UserId = userId,
                Enabled = true,
                Created = DateTime.Now,
                Immutable = false
            };

            _context.AppUserProvider.Add(result);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task AddToRoleAsync(AppUser user, string roleName)
        {
            var role = _context.Roles.Where(x => x.Name == roleName).SingleOrDefault();

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

        public override Task CreateAsync(AppUser user)
        {
            //TODO - figure out what to do with UserName requirement in Identity...
            user.UserName = user.Email;

            _context.AppUser.Add(user);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task DeleteAsync(Guid userId)
        {
            var user = _context.AppUser.Where(x => x.Id == userId).Single();

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
        
        public AppUser FindById(Guid userId)
        {
            return _context.AppUser.Where(x => x.Id == userId).SingleOrDefault();
        }

        public AppUser FindByName(string userName)
        {
            return _context.AppUser.Where(x => x.Email == userName).SingleOrDefault();
        }

        public IEnumerable<AppUser> Get(Expression<Func<AppUser, bool>> filter = null,
            Func<IQueryable<AppUser>, IOrderedQueryable<AppUser>> orderBy = null, string includes = "")
        {
            IQueryable<AppUser> query = _data;

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public IList<AppUser> GetAll()
        {
            return _context.AppUser.ToList();
        }

        public override Task<IList<Claim>> GetClaimsAsync(AppUser user)
        {
            IList<Claim> result = new List<Claim>();
            var claims = _context.AppUserClaim.Where(x => x.UserId == user.Id).ToList();

            if (claims == null)
                throw new InvalidOperationException();

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

        public IList<AppUserClaim> GetClaimsAsync(Guid userId)
        {
            return _context.AppUserClaim.Where(x => x.UserId == userId).ToList();
        }

        public override Task<string> GetEmailAsync(AppUser user)
        {
            return Task.FromResult(user.Email);
        }

        public override Task<string> GetPhoneNumberAsync(AppUser user)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<IList<string>> GetProvidersAsync(AppUser user)
        {
            IList<string> result = new List<string>();
            var providers = _context.AppUserProvider.Where(x => x.UserId == user.Id).ToList();

            if (providers == null)
                throw new InvalidOperationException();

            foreach (AppUserProvider provider in providers)
                result.Add(_context.AppProvider.Where(x => x.Id == provider.ProviderId).Select(r => r.Name).Single());

            return Task.FromResult(result);
        }

        public override Task<IList<string>> GetRolesAsync(AppUser user)
        {
            IList<string> result = new List<string>();
            var roles = _context.AppUserRole.Where(x => x.UserId == user.Id).ToList();

            if (roles == null)
                throw new InvalidOperationException();

            foreach (AppUserRole role in roles)
                result.Add(_context.Roles.Where(x => x.Id == role.RoleId).Select(r => r.Name).Single());

            return Task.FromResult(result);
        }

        public override Task<bool> GetTwoFactorEnabledAsync(AppUser user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
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

        public override Task RemoveClaimAsync(AppUser user, Claim claim)
        {
            var result = _context.AppUserClaim.Where(x => x.UserId == user.Id
                && x.ClaimType == claim.Type
                && x.ClaimValue == claim.Value).SingleOrDefault();

            if (result == null)
                throw new ArgumentNullException();

            _context.AppUserClaim.Remove(result);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task RemoveFromProviderAsync(AppUser user, string providerName)
        {
            var provider = _context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault();

            if (provider == null)
                throw new ArgumentNullException();

            var result = _context.AppUserProvider.Where(x => x.UserId == user.Id && x.ProviderId == provider.Id).Single();

            _context.AppUserProvider.Remove(result);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task RemoveFromRoleAsync(AppUser user, string roleName)
        {
            var role = _context.Roles.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            var result = _context.AppUserRole.Where(x => x.UserId == user.Id && x.RoleId == role.Id).Single();

            _context.AppUserRole.Remove(result);
            _context.SaveChanges();

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

        public override Task SetEmailConfirmedAsync(AppUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task SetImmutableEnabledAsync(AppUser user, bool enabled)
        {
            user.Immutable = enabled;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task SetPasswordConfirmedAsync(AppUser user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task SetTwoFactorEnabledAsync(AppUser user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task UpdateAsync(AppUser user)
        {
            var model = _context.AppUser.Where(x => x.Id == user.Id).Single();

            model.UserName = user.Email;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.LockoutEnabled = user.LockoutEnabled;
            model.LockoutEndDateUtc = user.LockoutEndDateUtc;
            model.Immutable = user.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}