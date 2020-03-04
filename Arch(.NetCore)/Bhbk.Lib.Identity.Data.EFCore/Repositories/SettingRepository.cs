using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class SettingRepository : GenericRepository<uvw_Settings>
    {
        public SettingRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Settings Update(uvw_Settings model)
        {
            var entity = _context.Set<uvw_Settings>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.ConfigKey = model.ConfigKey;
            entity.ConfigValue = model.ConfigValue;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
