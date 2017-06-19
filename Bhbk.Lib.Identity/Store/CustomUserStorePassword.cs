using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    public partial class CustomUserStore : IUserPasswordStore<AppUser, Guid>
    {
        public override Task<string> GetPasswordHashAsync(AppUser user)
        {
            return Task.FromResult(_context.Users.Where(x => x.Id == user.Id).Single().PasswordHash);
        }

        public override Task<bool> HasPasswordAsync(AppUser user)
        {
            var result = _context.Users.Where(x => x.Id == user.Id || x.UserName == user.UserName).SingleOrDefault();

            if (result == null)
                return Task.FromResult(false);
            else
            {
                if (string.IsNullOrEmpty(result.PasswordHash))
                    return Task.FromResult(false);

                return Task.FromResult(true);
            }
        }

        public override Task SetPasswordHashAsync(AppUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
