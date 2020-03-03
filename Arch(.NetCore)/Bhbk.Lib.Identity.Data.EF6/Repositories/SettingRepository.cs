using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class SettingRepository : GenericRepository<uvw_Settings>
    {
        public SettingRepository(IdentityEntities context)
            : base(context) { }
    }
}
