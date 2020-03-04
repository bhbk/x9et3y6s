using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class QueueTextRepository : GenericRepository<tbl_QueueTexts>
    {
        public QueueTextRepository(IdentityEntities context)
            : base(context) { }
    }
}
