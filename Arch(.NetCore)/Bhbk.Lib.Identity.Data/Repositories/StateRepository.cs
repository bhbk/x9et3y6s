﻿using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class StateRepository : GenericRepository<tbl_States>
    {
        public StateRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override tbl_States Update(tbl_States model)
        {
            var entity = _context.Set<tbl_States>().Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.LastPolling = model.LastPolling;
            entity.StateConsume = model.StateConsume;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}