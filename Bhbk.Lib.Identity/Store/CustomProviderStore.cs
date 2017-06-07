using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    public class CustomProviderStore
    {
        private CustomIdentityDbContext _context;

        public CustomProviderStore(CustomIdentityDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public Task CreateAsync(AppProvider provider)
        {
            provider.Created = DateTime.Now;

            _context.AppProvider.Add(provider);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task DeleteAsync(AppProvider provider)
        {
            _context.AppProvider.Remove(provider);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<AppProvider> FindByIdAsync(Guid providerId)
        {
            return Task.FromResult(_context.AppProvider.Where(x => x.Id == providerId).SingleOrDefault());
        }

        public Task<AppProvider> FindByNameAsync(string providerName)
        {
            return Task.FromResult(_context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault());
        }

        public Task<IList<string>> GetUsersAsync(AppProvider provider)
        {
            IList<string> result = new List<string>();
            var users = _context.AppUserProvider.Where(x => x.ProviderId == provider.Id).ToList();

            if (users == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppUserProvider user in users)
                    result.Add(_context.Users.Where(x => x.Id == user.UserId).Select(r => r.Email).Single());

                return Task.FromResult(result);
            }
        }

        public Task<bool> IsInProviderAsync(AppProvider provider, string roleName)
        {
            var user = _context.AppProvider.Where(x => x.Name == roleName).SingleOrDefault();

            if (user == null)
                throw new ArgumentNullException();

            else if (_context.AppUserProvider.Any(x => x.ProviderId == provider.Id && x.UserId == user.Id))
                return Task.FromResult(true);

            else
                return Task.FromResult(false);
        }

        public bool IsValidProvider(AppProvider provider)
        {
            var result = _context.AppProvider.Where(x => x.Id == provider.Id || x.Name == provider.Name).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public bool IsValidProvider(Guid provider)
        {
            var result = _context.AppProvider.Where(x => x.Id == provider).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public bool IsValidProvider(Guid provider, out AppProvider result)
        {
            result = _context.AppProvider.Where(x => x.Id == provider).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public Task UpdateAsync(AppProvider provider)
        {
            provider.LastUpdated = DateTime.Now;

            _context.Entry(provider).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
