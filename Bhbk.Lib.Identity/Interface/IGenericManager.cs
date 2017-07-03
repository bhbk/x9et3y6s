using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Interface
{
    public interface IGenericManager<TEntity, TKey>
        where TEntity : class
    {
        Task<TEntity> CreateAsync(TEntity entity);
        Task<bool> DeleteAsync(TKey key);
        Task<TEntity> FindByIdAsync(TKey key);
        Task<TEntity> UpdateAsync(TEntity entity);
    }
}
