using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
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
