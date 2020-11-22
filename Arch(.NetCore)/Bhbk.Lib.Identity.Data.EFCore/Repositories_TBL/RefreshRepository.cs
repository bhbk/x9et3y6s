using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_TBL;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_TBL
{
    public class RefreshRepository : GenericRepository<tbl_Refresh>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Refresh Update(tbl_Refresh entity)
        {
            throw new NotImplementedException();
        }
    }
}
