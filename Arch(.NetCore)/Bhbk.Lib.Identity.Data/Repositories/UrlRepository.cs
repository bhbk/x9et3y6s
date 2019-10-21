using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class UrlRepository : GenericRepository<tbl_Urls>
    {
        public UrlRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }
    }
}
