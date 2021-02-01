using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories_Tbl
{
    public class AuthActivityRepository : GenericRepository<tbl_AuthActivity>
    {
        public AuthActivityRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_AuthActivity Update(tbl_AuthActivity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<tbl_AuthActivity> Update(IEnumerable<tbl_AuthActivity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
