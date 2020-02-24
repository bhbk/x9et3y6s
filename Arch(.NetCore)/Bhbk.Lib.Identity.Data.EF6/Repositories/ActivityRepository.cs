using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class ActivityRepository : GenericRepository<tbl_Activities>
    {
        public ActivityRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }
    }
}
