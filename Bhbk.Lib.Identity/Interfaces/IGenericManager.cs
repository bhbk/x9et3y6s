using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IGenericManager<TEntity, TKey>
        where TEntity : class
    {
        Task<TEntity> CreateAsync(TEntity entity);
        Task<bool> DeleteAsync(TEntity entity);
        Task<TEntity> FindByIdAsync(TKey key);
        Task<TEntity> UpdateAsync(TEntity entity);
    }
}
