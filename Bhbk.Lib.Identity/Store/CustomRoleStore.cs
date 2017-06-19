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
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    //https://msdn.microsoft.com/en-us/library/dn613257(v=vs.108).aspx
    public class CustomRoleStore : RoleStore<AppRole, Guid, AppUserRole>
    {
        private CustomIdentityDbContext _context;
        private DbSet<AppRole> _data;
        private ModelFactory _factory;

        public CustomRoleStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _data = context.Set<AppRole>();
            _factory = new ModelFactory(context);
        }

        public Task CreateAsync(RoleModel.Create role)
        {
            var create = _factory.Create.DoIt(role);
            var model = _factory.Devolve.DoIt(create);

            _context.AppRole.Add(model);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task DeleteAsync(Guid roleId)
        {
            var role = _context.AppRole.Where(x => x.Id == roleId).Single();

            _context.AppRole.Remove(role);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        private Task<RoleModel.Model> Find(AppRole role)
        {
            if (role == null)
                return Task.FromResult<RoleModel.Model>(null);
            else
                return Task.FromResult(_factory.Evolve.DoIt(role));
        }

        public Task<RoleModel.Model> FindById(Guid roleId)
        {
            return Find(_context.AppRole.Where(x => x.Id == roleId).SingleOrDefault());
        }

        public Task<RoleModel.Model> FindByName(string roleName)
        {
            return Find(_context.AppRole.Where(x => x.Name == roleName).SingleOrDefault());
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

        public Task<IList<RoleModel.Model>> GetAllAsync()
        {
            IList<RoleModel.Model> result = new List<RoleModel.Model>();
            var roles = _context.AppRole.ToList();

            if (roles == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppRole role in roles)
                    result.Add(_factory.Evolve.DoIt(role));

                return Task.FromResult(result);
            }
        }

        public Task<IList<UserModel.Model>> GetUsersAsync(Guid roleId)
        {
            IList<UserModel.Model> result = new List<UserModel.Model>();
            var list = _context.AppUserRole.Where(x => x.RoleId == roleId).ToList();

            if (list == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppUserRole entry in list)
                {
                    var user = _context.AppUser.Where(x => x.Id == entry.UserId).Single();

                    result.Add(_factory.Evolve.DoIt(user));
                }

                return Task.FromResult(result);
            }
        }

        public Task UpdateAsync(RoleModel.Update role)
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