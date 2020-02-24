using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class RefreshRepository : GenericRepository<tbl_Refreshes>
    {
        public RefreshRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override tbl_Refreshes Update(tbl_Refreshes entity)
        {
            throw new NotImplementedException();
        }
    }
}
