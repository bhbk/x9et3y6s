using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Interface
{
    public interface IGenericStore<TEntity, TKey> : IDisposable
        where TEntity : class
    {
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null, string includes = "");
        void Attach(TEntity entity);
        TEntity Create(TEntity entity);
        bool Delete(TKey key);
        bool Exists(TKey key);
        TEntity FindById(TKey key);
        void LoadCollection(TEntity entity, string collection);
        void LoadReference(TEntity entity, string reference);
        void Save();
        TEntity Update(TEntity entity);
    }
}
