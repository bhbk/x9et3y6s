using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class UrlRepository : GenericRepository<tbl_Urls>
    {
        public UrlRepository(IdentityEntities context)
            : base(context) { }
    }
}
