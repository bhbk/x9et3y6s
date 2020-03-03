using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class QueueTextRepository : GenericRepository<tbl_QueueTexts>
    {
        public QueueTextRepository(IdentityEntities context)
            : base(context) { }
    }
}
