using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    public partial class CustomUserStore : IUserSecurityStampStore<AppUser, Guid>
    {
        public override Task<string> GetSecurityStampAsync(AppUser user)
        {
            return Task.FromResult(_context.Users.Where(x => x.Id == user.Id).Single().SecurityStamp);
        }

        public override Task SetSecurityStampAsync(AppUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            user.LastUpdated = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
