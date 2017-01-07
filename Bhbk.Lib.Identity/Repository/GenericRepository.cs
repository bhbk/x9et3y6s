using Bhbk.Lib.Identity.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{   
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public abstract class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>, IDisposable
        where TEntity : class
    {
        private bool _disposed = false;
        protected CustomIdentityDbContext _context;
        protected DbSet<TEntity> _data;

        public GenericRepository(CustomIdentityDbContext context)
        {
            this._context = context;
            this._data = context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includes = "")
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

        public virtual TEntity Find(TKey id)
        {
            return _data.Find(id);
        }

        public virtual async Task<TEntity> FindAsync(TKey id)
        {
            return await _data.FindAsync(id);
        }

        public virtual void Create(TEntity entity)
        {
            _data.Add(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entity = _data.Find(id);
            Delete(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                _data.Attach(entity);

            _data.Remove(entity);
        }

        public virtual void Attach(TEntity entity)
        {
            _data.Attach(entity);
        }

        public virtual bool Exists(TKey key)
        {
            throw new NotImplementedException();
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

        public virtual async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void Update(TEntity entity)
        {
            _data.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                    _context.Dispose();
            }

            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
