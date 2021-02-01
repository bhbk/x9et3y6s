using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories
{
    public class AuthActivityRepository : GenericRepository<E_AuthActivity>
    {
        public AuthActivityRepository(IdentityEntities context)
            : base(context) { }

        public override E_AuthActivity Update(E_AuthActivity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<E_AuthActivity> Update(IEnumerable<E_AuthActivity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
