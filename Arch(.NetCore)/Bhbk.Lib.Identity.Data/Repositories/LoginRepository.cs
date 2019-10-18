using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class LoginRepository : GenericRepositoryAsync<tbl_Logins>
    {
        public LoginRepository(_DbContext context, InstanceContext instance)
            : base(context, instance) { }

        public override async Task<tbl_Logins> UpdateAsync(tbl_Logins login)
        {
            var entity = _context.Set<tbl_Logins>().Where(x => x.Id == login.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = login.Name;
            entity.Description = login.Description;
            entity.LoginKey = login.LoginKey;
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = login.Enabled;
            entity.Immutable = login.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
