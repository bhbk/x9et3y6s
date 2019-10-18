using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : GenericRepositoryAsync<tbl_Roles>
    {
        public RoleRepository(_DbContext context, InstanceContext instance)
            : base(context, instance) { }

        public  override async Task<tbl_Roles> UpdateAsync(tbl_Roles model)
        {
            var entity = _context.Set<tbl_Roles>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Enabled = model.Enabled;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}