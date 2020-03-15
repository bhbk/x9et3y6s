﻿using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class ActivityRepository : GenericRepository<tbl_Activities>
    {
        public ActivityRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Activities Update(tbl_Activities entity)
        {
            throw new NotImplementedException();
        }        
    }
}