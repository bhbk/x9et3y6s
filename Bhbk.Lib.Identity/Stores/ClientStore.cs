using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Stores
{
    public class ClientStore : IGenericStore<AppClient, Guid>
    {
        private AppDbContext _context;
        public ModelFactory Mf;

        public ClientStore(AppDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;

            Mf = new ModelFactory(context);
        }

        public void Attach(AppClient entity)
        {
            throw new NotImplementedException();
        }

        public AppClient Create(AppClient entity)
        {
            var result = _context.AppClient.Add(entity);
            _context.SaveChanges();

            return result.Entity;
        }

        public bool Delete(Guid key)
        {
            var client = _context.AppClient.Where(x => x.Id == key).Single();
            var audiences = _context.AppAudience.Where(x => x.ClientId == key);
            var roles = _context.AppRole.Where(x => x.AudienceId == audiences.FirstOrDefault().Id);

            _context.AppRole.RemoveRange(roles);
            _context.AppAudience.RemoveRange(audiences);
            _context.AppClient.Remove(client);
            _context.SaveChanges();

            return true;
            //return Exists(clientId);
        }

        public bool Exists(Guid key)
        {
            return _context.AppClient.Any(x => x.Id == key);
        }

        public bool Exists(string name)
        {
            return _context.AppClient.Any(x => x.Name == name);
        }

        public AppClient FindById(Guid key)
        {
            return _context.AppClient.Where(x => x.Id == key).SingleOrDefault();
        }

        public AppClient FindByName(string name)
        {
            return _context.AppClient.Where(x => x.Name == name).SingleOrDefault();
        }

        public IList<AppClient> GetAll()
        {
            return _context.AppClient.ToList();
        }

        public IEnumerable<AppClient> Get(Expression<Func<AppClient, bool>> filter = null,
            Func<IQueryable<AppClient>, IOrderedQueryable<AppClient>> orderBy = null, string includes = "")
        {
            IQueryable<AppClient> query = _context.AppClient.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public IList<AppAudience> GetAudiences(Guid key)
        {
            return _context.AppAudience.Where(x => x.ClientId == key).ToList();
        }

        public void LoadCollection(AppClient entity, string collection)
        {
            throw new NotImplementedException();
        }

        public void LoadReference(AppClient entity, string reference)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public AppClient Update(AppClient entity)
        {
            var model = _context.AppClient.Where(x => x.Id == entity.Id).Single();

            model.Name = entity.Name;
            model.Description = entity.Description;
            model.Enabled = entity.Enabled;
            model.Immutable = entity.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return model;
        }
    }
}
