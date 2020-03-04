using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class ClaimRepository : GenericRepository<tbl_Claims>
    {
        public ClaimRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Claims Update(tbl_Claims model)
        {
            var entity = _context.Set<tbl_Claims>()
                .Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Subject = model.Subject;
            entity.Type = model.Type;
            entity.Value = model.Value;
            entity.ValueType = model.ValueType;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = model.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
