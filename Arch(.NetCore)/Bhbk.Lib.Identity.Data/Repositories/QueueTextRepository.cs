using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class QueueTextRepository : GenericRepository<tbl_QueueTexts>
    {
        public QueueTextRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }
    }
}
