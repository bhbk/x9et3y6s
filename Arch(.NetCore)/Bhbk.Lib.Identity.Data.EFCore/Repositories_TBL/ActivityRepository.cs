using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_TBL;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_TBL
{
    public class ActivityRepository : GenericRepository<tbl_Activity>
    {
        public ActivityRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Activity Update(tbl_Activity entity)
        {
            throw new NotImplementedException();
        }        
    }
}
