using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using System;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class RefreshRepository : GenericRepository<tbl_Refreshes>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Refreshes Update(tbl_Refreshes entity)
        {
            throw new NotImplementedException();
        }
    }
}
