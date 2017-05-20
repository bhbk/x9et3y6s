using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    //https://msdn.microsoft.com/en-us/library/dn613257(v=vs.108).aspx
    public class CustomRoleStore : RoleStore<AppRole, Guid, AppUserRole>
    {
        private CustomIdentityDbContext _context;

        public CustomRoleStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public override Task CreateAsync(AppRole role)
        {
            role.Created = DateTime.Now;
            role.Immutable = false;

            _context.Roles.Add(role);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task DeleteAsync(AppRole role)
        {
            _context.Roles.Remove(role);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public bool IsValidRole(AppRole role)
        {
            var result = _context.Roles.Where(x => x.Id == role.Id || x.Name == role.Name).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public override Task UpdateAsync(AppRole role)
        {
            role.LastUpdated = DateTime.Now;
            role.Immutable = false;

            _context.Entry(role).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}