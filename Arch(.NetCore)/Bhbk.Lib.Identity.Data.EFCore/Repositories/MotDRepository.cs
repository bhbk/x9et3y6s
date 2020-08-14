﻿using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class MOTDRepository : GenericRepository<uvw_MOTD>
    {
        public MOTDRepository(IdentityEntities context)
            : base(context) { }
    }
}
