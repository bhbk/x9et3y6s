using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class QueueTextRepository : GenericRepository<tbl_QueueText>
    {
        public QueueTextRepository(IdentityEntities context)
            : base(context) { }
    }
}
