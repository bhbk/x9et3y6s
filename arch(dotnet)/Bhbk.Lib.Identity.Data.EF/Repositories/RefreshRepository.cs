using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using System;

namespace Bhbk.Lib.Identity.Data.EF.Repositories
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
