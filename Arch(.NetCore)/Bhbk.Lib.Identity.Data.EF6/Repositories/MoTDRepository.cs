using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class MOTDRepository : GenericRepository<uvw_MOTDs>
    {
        public MOTDRepository(IdentityEntities context)
            : base(context) { }

    }
}
