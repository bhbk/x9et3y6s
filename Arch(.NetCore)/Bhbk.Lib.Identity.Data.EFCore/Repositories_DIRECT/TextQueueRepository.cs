using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class TextQueueRepository : GenericRepository<tbl_TextQueue>
    {
        public TextQueueRepository(IdentityEntities context)
            : base(context) { }
    }
}
