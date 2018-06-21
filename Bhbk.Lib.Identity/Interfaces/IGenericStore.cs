using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IGenericStore<TEntity, TKey>
        where TEntity : class
    {
        void Attach(TEntity entity);
        TEntity Create(TEntity entity);
        bool Delete(TEntity entity);
        bool Exists(TKey key);
        TEntity FindById(TKey key);
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null, string includes = "");
        void LoadCollection(TEntity entity, string collection);
        void LoadReference(TEntity entity, string reference);
        void Save();
        TEntity Update(TEntity entity);
    }
}
