using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class MotDRepository : GenericRepository<tbl_MOTDs>
    {
        public MotDRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }
    }
}
