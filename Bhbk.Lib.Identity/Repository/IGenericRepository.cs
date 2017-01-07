using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repository
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public interface IGenericRepository<TEntity, TKey> : IDisposable
        where TEntity : class
    {
        IEnumerable<TEntity> Get(
             Expression<Func<TEntity, bool>> filter = null,
             Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
             string includes = "");
        void Attach(TEntity entity);
        void Create(TEntity entity);
        void Delete(TEntity entity);
        bool Exists(TKey key);
        TEntity Find(TKey key);
        Task<TEntity> FindAsync(TKey key);
        void LoadCollection(TEntity entity, string collection);
        void LoadReference(TEntity entity, string reference);
        void Save();
        Task SaveAsync();
        void Update(TEntity entity);
    }
}
