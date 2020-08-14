using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class QueueEmailRepository : GenericRepository<tbl_QueueEmail>
    {
        public QueueEmailRepository(IdentityEntities context)
            : base(context) { }
    }
}
