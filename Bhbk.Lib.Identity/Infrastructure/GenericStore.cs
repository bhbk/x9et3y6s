using Bhbk.Lib.Identity.Interface;
using Bhbk.Lib.Identity.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public abstract class GenericStore<TEntity, TKey> : IGenericStore<TEntity, TKey>, IDisposable
        where TEntity : class
    {
        private bool _disposed = false;
        protected CustomIdentityDbContext _context;
        protected DbSet<TEntity> _data;

        public GenericStore(CustomIdentityDbContext context)
        {
            this._context = context;
            this._data = context.Set<TEntity>();
        }

        public virtual void Attach(TEntity entity)
        {
            _data.Attach(entity);
        }

        public virtual TEntity Create(TEntity entity)
        {
            return _data.Add(entity);
        }

        public virtual bool Delete(TKey key)
        {
            var entity = _data.Find(key);

            if (_context.Entry(entity).State == EntityState.Detached)
                _data.Attach(entity);

            _data.Remove(entity);

            return Exists(key);
        }

        public virtual bool Exists(TKey key)
        {
            if (_data.Find(key) == null)
                return false;
            else
                return true;
        }

        public virtual TEntity FindById(TKey id)
        {
            return _data.Find(id);
        }

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includes = "")
        {
            IQueryable<TEntity> query = _data;

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public virtual void LoadCollection(TEntity entity, string collection)
        {
            if (!_context.Entry(entity).Collection(collection).IsLoaded)
                _context.Entry(entity).Collection(collection).Load();
        }

        public virtual void LoadReference(TEntity entity, string reference)
        {
            if (!_context.Entry(entity).Reference(reference).IsLoaded)
                _context.Entry(entity).Reference(reference).Load();
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }

        public virtual TEntity Update(TEntity entity)
        {
            _data.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //free any managed objects here...
                _context.Dispose();
            }

            //free any unmanaged objects here...
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
