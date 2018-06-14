using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Stores
{
    public class LoginStore : IGenericStore<AppLogin, Guid>
    {
        private readonly AppDbContext _context;

        public LoginStore(AppDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public void Attach(AppLogin entity)
        {
            throw new NotImplementedException();
        }

        public AppLogin Create(AppLogin entity)
        {
            var result = _context.AppLogin.Add(entity);
            _context.SaveChanges();

            return result.Entity;
        }

        public bool Delete(Guid key)
        {
            var login = _context.AppLogin.Where(x => x.Id == key).Single();

            _context.AppLogin.Remove(login);
            _context.SaveChanges();

            return true;
            //return Exists(key);
        }

        public bool Exists(Guid key)
        {
            return _context.AppLogin.Any(x => x.Id == key);
        }

        public bool Exists(string name)
        {
            return _context.AppLogin.Any(x => x.LoginProvider == name);
        }

        public AppLogin FindById(Guid key)
        {
            return _context.AppLogin.Where(x => x.Id == key).SingleOrDefault();
        }

        public AppLogin FindByName(string name)
        {
            return _context.AppLogin.Where(x => x.LoginProvider == name).SingleOrDefault();
        }

        public IList<AppLogin> Get()
        {
            return _context.AppLogin.ToList();
        }

        public IList<AppUser> GetUsers(Guid key)
        {
            var result = (IList<string>)_context.AppLogin
                .Join(_context.AppUserLogin, x => x.Id, y => y.LoginId, (login1, user1) => new {
                    LoginId = login1.Id,
                    UserId = user1.UserId
                })
                .Where(x => x.LoginId == key)
                .Select(x => x.UserId.ToString().ToLower())
                .Distinct()
                .ToList();

            return _context.AppUser.Where(x => result.Contains(x.Id.ToString())).ToList();
        }

        public IEnumerable<AppLogin> Get(Expression<Func<AppLogin, bool>> filter = null,
            Func<IQueryable<AppLogin>, IOrderedQueryable<AppLogin>> orderBy = null, string includes = "")
        {
            IQueryable<AppLogin> query = _context.AppLogin.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public void LoadCollection(AppLogin entity, string collection)
        {
            throw new NotImplementedException();
        }

        public void LoadReference(AppLogin entity, string reference)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public AppLogin Update(AppLogin entity)
        {
            var model = _context.AppLogin.Find(entity.Id);

            model.LoginProvider = entity.LoginProvider;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return model;
        }
    }
}
