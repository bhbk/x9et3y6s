using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    //https://msdn.microsoft.com/en-us/library/dn613257(v=vs.108).aspx
    public class CustomRoleStore : RoleStore<AppRole, Guid, AppUserRole>
    {
        private CustomIdentityDbContext _context;
        private DbSet<AppRole> _data;
        public ModelFactory Mf;

        public CustomRoleStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _data = context.Set<AppRole>();
            Mf = new ModelFactory(context);
        }

        
        public override Task CreateAsync(AppRole role)
        {
            _context.AppRole.Add(role);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task DeleteAsync(AppRole role)
        {
            _context.AppRole.Remove(role);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public AppRole FindById(Guid roleId)
        {
            return _context.AppRole.Where(x => x.Id == roleId).SingleOrDefault();
        }

        public AppRole FindByName(string roleName)
        {
            return _context.AppRole.Where(x => x.Name == roleName).SingleOrDefault();
        }

        public bool Exists(Guid roleId)
        {
            return _context.AppRole.Any(x => x.Id == roleId);
        }

        public bool Exists(string roleName)
        {
            return _context.AppRole.Any(x => x.Name == roleName);
        }

        public IEnumerable<AppRole> Get(Expression<Func<AppRole, bool>> filter = null,
            Func<IQueryable<AppRole>, IOrderedQueryable<AppRole>> orderBy = null, string includes = "")
        {
            IQueryable<AppRole> query = _data;

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public IList<AppRole> GetAll()
        {
            return _context.AppRole.ToList();
        }

        public IList<AppUser> GetUsersAsync(Guid roleId)
        {            
            IList<AppUser> result = new List<AppUser>();
            var list = _context.AppUserRole.Where(x => x.RoleId == roleId).ToList();

            if (list == null)
                throw new InvalidOperationException();

            foreach (AppUserRole entry in list)
                result.Add(_context.AppUser.Where(x => x.Id == entry.UserId).Single());

            return result;
        }

        public override Task UpdateAsync(AppRole role)
        {
            var model = _context.AppRole.Where(x => x.Id == role.Id).Single();

            model.Name = role.Name;
            model.Description = role.Description;
            model.Enabled = role.Enabled;
            model.Immutable = role.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}