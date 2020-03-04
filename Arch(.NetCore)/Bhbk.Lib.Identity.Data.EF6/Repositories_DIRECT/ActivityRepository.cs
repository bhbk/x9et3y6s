using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using System;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class ActivityRepository : GenericRepository<tbl_Activities>
    {
        public ActivityRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Activities Update(tbl_Activities entity)
        {
            throw new NotImplementedException();
        }        
    }
}
