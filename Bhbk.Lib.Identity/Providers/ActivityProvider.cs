using Bhbk.Lib.Identity.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bhbk.Lib.Identity.Providers
{
    public abstract class ActivityProvider
    {

    }

    public enum ActivityType
    {
        Create,
        Read,
        Update,
        Delete,
        StsAccessToken,
        StsAuthorizationCode,
        StsRefreshToken,
    }

    public class ActivityProvider<TEntity> : ActivityProvider
        where TEntity : class, IGenericActivity, new()
    {
        private readonly DbContext _context;

        public ActivityProvider(DbContext context)
        {
            _context = context;
        }

        public void Commit(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
        }
    }
}
