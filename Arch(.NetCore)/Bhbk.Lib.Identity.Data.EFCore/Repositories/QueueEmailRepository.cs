using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class QueueEmailRepository : GenericRepository<tbl_QueueEmails>
    {
        public QueueEmailRepository(IdentityEntities context)
            : base(context) { }
    }
}
