using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Stores
{
    public class ActivityStore : IGenericStore<AppActivity, Guid>
    {
        private readonly AppDbContext _context;

        public ActivityStore(AppDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public void Attach(AppActivity entity)
        {
            throw new NotImplementedException();
        }

        public AppActivity Create(AppActivity entity)
        {
            var result = _context.AppActivity.Add(entity);
            _context.SaveChanges();

            return result.Entity;
        }

        public bool Delete(AppActivity entity)
        {
            var activity = _context.AppActivity.Where(x => x.Id == entity.Id).Single();

            _context.AppActivity.Remove(activity);
            _context.SaveChanges();

            return !Exists(entity.Id);
        }

        public bool Exists(Guid key)
        {
            return _context.AppActivity.Any(x => x.Id == key);
        }

        public AppActivity FindById(Guid key)
        {
            return _context.AppActivity.Where(x => x.Id == key).SingleOrDefault();
        }

        public IEnumerable<AppActivity> Get(Expression<Func<AppActivity, bool>> filter = null,
            Func<IQueryable<AppActivity>, IOrderedQueryable<AppActivity>> orderBy = null, string includes = "")
        {
            IQueryable<AppActivity> query = _context.AppActivity.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public void LoadCollection(AppActivity entity, string collection)
        {
            throw new NotImplementedException();
        }

        public void LoadReference(AppActivity entity, string reference)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public AppActivity Update(AppActivity entity)
        {
            throw new NotImplementedException();
        }
    }
}
