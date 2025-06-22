using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using System;

namespace Bhbk.Lib.Identity.Data.EF.Repositories
{
    public class AuthActivityRepository : GenericRepository<tbl_AuthActivity>
    {
        public AuthActivityRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_AuthActivity Update(tbl_AuthActivity entity)
        {
            throw new NotImplementedException();
        }        
    }
}
