using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
//using System.Data;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class StateRepository : GenericRepository<tbl_States>
    {
        public StateRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_States Update(tbl_States model)
        {
            var entity = _context.Set<tbl_States>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.LastPolling = model.LastPolling;
            entity.StateConsume = model.StateConsume;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}
