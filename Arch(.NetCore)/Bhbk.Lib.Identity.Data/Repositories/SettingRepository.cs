using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class SettingRepository : GenericRepositoryAsync<tbl_Settings>
    {
        public SettingRepository(_DbContext context, InstanceContext instance)
            : base(context, instance) { }

        public override async ValueTask<tbl_Settings> UpdateAsync(tbl_Settings model)
        {
            var entity = _context.Set<tbl_Settings>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.ConfigKey = model.ConfigKey;
            entity.ConfigValue = model.ConfigValue;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
