using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories
{
    public class RefreshRepository : GenericRepository<E_Refresh>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override E_Refresh Update(E_Refresh entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<E_Refresh> Update(IEnumerable<E_Refresh> entities)
        {
            throw new NotImplementedException();
        }
    }
}
