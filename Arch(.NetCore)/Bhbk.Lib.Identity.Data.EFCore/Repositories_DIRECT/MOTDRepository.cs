using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class MOTDRepository : GenericRepository<tbl_MOTDs>
    {
        public MOTDRepository(IdentityEntities context)
            : base(context) { }
    }
}
