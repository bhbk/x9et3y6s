using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class UrlRepository : GenericRepositoryAsync<tbl_Urls>
    {
        public UrlRepository(_DbContext context, InstanceContext instance)
            : base(context, instance) { }
    }
}
