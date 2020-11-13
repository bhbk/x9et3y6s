using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class LoginRepository : GenericRepository<tbl_Login>
    {
        public LoginRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Login Update(tbl_Login login)
        {
            var entity = _context.Set<tbl_Login>()
                .Where(x => x.Id == login.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = login.Name;
            entity.Description = login.Description;
            entity.LoginKey = login.LoginKey;
            entity.LastUpdatedUtc = DateTime.UtcNow;
            entity.IsEnabled = login.IsEnabled;
            entity.IsDeletable = login.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
