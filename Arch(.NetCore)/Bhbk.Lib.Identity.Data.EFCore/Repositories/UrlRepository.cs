using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class UrlRepository : GenericRepository<tbl_Urls>
    {
        public UrlRepository(IdentityEntities context)
            : base(context) { }
    }
}
