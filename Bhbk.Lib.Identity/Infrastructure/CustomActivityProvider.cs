using Bhbk.Lib.Identity.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public abstract class CustomActivityProvider
    {

    }

    public enum ActivityType
    {
        Create,
        Read,
        Update,
        Delete,
        StsAccess,
        StsRefresh
    }

    public class CustomActivityProvider<TEntity> : CustomActivityProvider
        where TEntity : class, IActivityEntry, new()
    {
        private readonly DbContext _context;

        public CustomActivityProvider(DbContext context)
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
