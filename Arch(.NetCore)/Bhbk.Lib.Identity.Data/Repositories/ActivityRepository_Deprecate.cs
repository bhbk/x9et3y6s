using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using System;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class ActivityRepository_Deprecate : GenericRepository<tbl_Activities>
    {
        public ActivityRepository_Deprecate(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override tbl_Activities Update(tbl_Activities entity)
        {
            throw new NotImplementedException();
        }        
    }
}
