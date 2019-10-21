using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class SettingRepository : GenericRepository<tbl_Settings>
    {
        public SettingRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

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
