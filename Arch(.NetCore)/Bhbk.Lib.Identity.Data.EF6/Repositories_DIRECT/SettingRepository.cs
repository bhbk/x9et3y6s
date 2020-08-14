using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class SettingRepository : GenericRepository<tbl_Setting>
    {
        public SettingRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Setting Update(tbl_Setting model)
        {
            var entity = _context.Set<tbl_Setting>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.ConfigKey = model.ConfigKey;
            entity.ConfigValue = model.ConfigValue;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}
