using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Stores
{
    public class AudienceStore : IGenericStore<AppAudience, Guid>
    {
        private readonly AppDbContext _context;

        public AudienceStore(AppDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public void Attach(AppAudience entity)
        {
            throw new NotImplementedException();
        }

        public AppAudienceUri AddUri(AppAudienceUri entity)
        {
            var result = _context.AppAudienceUri.Add(entity);
            _context.SaveChanges();

            return result.Entity;
        }

        public AppAudience Create(AppAudience entity)
        {
            var result = _context.AppAudience.Add(entity);
            _context.SaveChanges();

            return result.Entity;
        }

        public bool Delete(AppAudience entity)
        {
            var audience = _context.AppAudience.Where(x => x.Id == entity.Id).Single();
            var roles = _context.AppRole.Where(x => x.AudienceId == entity.Id);

            _context.AppRole.RemoveRange(roles);
            _context.AppAudience.Remove(audience);
            _context.SaveChanges();

            return !Exists(entity.Id);
        }

        public bool Exists(Guid key)
        {
            return _context.AppAudience.Any(x => x.Id == key);
        }

        public bool Exists(string name)
        {
            return _context.AppAudience.Any(x => x.Name == name);
        }

        public AppAudience FindById(Guid key)
        {
            return _context.AppAudience.Where(x => x.Id == key).SingleOrDefault();
        }

        public AppAudience FindByName(string name)
        {
            return _context.AppAudience.Where(x => x.Name == name).SingleOrDefault();
        }

        public IQueryable<AppAudience> Get()
        {
            return _context.AppAudience.AsQueryable();
        }

        public IQueryable<AppAudience> Get(Expression<Func<AppAudience, bool>> filter = null,
            Func<IQueryable<AppAudience>, IOrderedQueryable<AppAudience>> orderBy = null, string includes = "")
        {
            IQueryable<AppAudience> query = _context.AppAudience.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query);

            else
                return query;
        }

        public IQueryable<AppRole> GetRoles(Guid key)
        {
            return _context.AppRole.Where(x => x.AudienceId == key).AsQueryable();
        }

        public void LoadCollection(AppAudience entity, string collection)
        {
            throw new NotImplementedException();
        }

        public void LoadReference(AppAudience entity, string reference)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public bool SetImmutableAsync(AppAudience audience, bool enabled)
        {
            audience.Immutable = enabled;
            audience.LastUpdated = DateTime.Now;

            _context.Entry(audience).State = EntityState.Modified;
            _context.SaveChanges();

            return true;
        }

        public AppAudience Update(AppAudience entity)
        {
            var mode = _context.AppAudience.Where(x => x.Id == entity.Id).Single();

            mode.ClientId = entity.ClientId;
            mode.Name = entity.Name;
            mode.Description = entity.Description;
            mode.AudienceType = entity.AudienceType;
            mode.Enabled = entity.Enabled;
            mode.LastUpdated = DateTime.Now;
            mode.Immutable = entity.Immutable;

            _context.Entry(mode).State = EntityState.Modified;
            _context.SaveChanges();

            return mode;
        }
    }
}
