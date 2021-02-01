using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories_Tbl
{
    public class RefreshRepository : GenericRepository<tbl_Refresh>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Refresh Update(tbl_Refresh entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<tbl_Refresh> Update(IEnumerable<tbl_Refresh> entities)
        {
            throw new NotImplementedException();
        }
    }
}
