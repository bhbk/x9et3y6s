﻿using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using System;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class LoginRepository : GenericRepository<tbl_Logins>
    {
        public LoginRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Logins Update(tbl_Logins login)
        {
            var entity = _context.Set<tbl_Logins>()
                .Where(x => x.Id == login.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = login.Name;
            entity.Description = login.Description;
            entity.LoginKey = login.LoginKey;
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = login.Enabled;
            entity.Immutable = login.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}
