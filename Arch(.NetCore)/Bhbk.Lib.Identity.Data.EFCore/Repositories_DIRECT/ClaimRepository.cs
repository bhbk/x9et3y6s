using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class ClaimRepository : GenericRepository<tbl_Claim>
    {
        public ClaimRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Claim Update(tbl_Claim model)
        {
            var entity = _context.Set<tbl_Claim>()
                .Where(x => x.Id == model.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Subject = model.Subject;
            entity.Type = model.Type;
            entity.Value = model.Value;
            entity.ValueType = model.ValueType;
            entity.LastUpdatedUtc = DateTime.UtcNow;
            entity.IsDeletable = model.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }
    }
}
