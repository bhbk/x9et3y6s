using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models_TSQL;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories_TSQL
{
    public class RefreshRepository : GenericRepository<uvw_Refresh>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Refresh Update(uvw_Refresh entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Refresh> Update(IEnumerable<uvw_Refresh> entities)
        {
            throw new NotImplementedException();
        }
    }
}
