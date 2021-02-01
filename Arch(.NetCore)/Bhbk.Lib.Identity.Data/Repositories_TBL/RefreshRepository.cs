using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using System;

namespace Bhbk.Lib.Identity.Data.Repositories_Tbl
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
