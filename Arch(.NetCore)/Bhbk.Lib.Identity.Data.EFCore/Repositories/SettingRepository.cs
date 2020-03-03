using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class SettingRepository : GenericRepository<tbl_Settings>
    {
        public SettingRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Settings Update(tbl_Settings model)
        {
            var entity = _context.Set<tbl_Settings>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.ConfigKey = model.ConfigKey;
            entity.ConfigValue = model.ConfigValue;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
