﻿using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class RefreshRepository : GenericRepository<tbl_Refreshes>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Refreshes Update(tbl_Refreshes entity)
        {
            throw new NotImplementedException();
        }
    }
}