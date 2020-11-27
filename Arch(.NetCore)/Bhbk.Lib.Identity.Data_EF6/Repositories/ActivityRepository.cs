using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories
{
    public class ActivityRepository : GenericRepository<uvw_Activity>
    {
        public ActivityRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Activity Update(uvw_Activity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Activity> Update(IEnumerable<uvw_Activity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
