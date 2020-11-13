using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : GenericRepository<tbl_Role>
    {
        public RoleRepository(IdentityEntities context)
            : base(context) { }

        public  override tbl_Role Update(tbl_Role model)
        {
            var entity = _context.Set<tbl_Role>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.IsEnabled = model.IsEnabled;
            entity.LastUpdatedUtc = DateTime.UtcNow;
            entity.IsDeletable = model.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}