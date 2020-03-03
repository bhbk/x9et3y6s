using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class QueueTextRepository : GenericRepository<uvw_QueueTexts>
    {
        public QueueTextRepository(IdentityEntities context)
            : base(context) { }
    }
}
