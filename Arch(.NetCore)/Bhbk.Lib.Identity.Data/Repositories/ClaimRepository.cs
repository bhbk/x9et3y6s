using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class ClaimRepository : GenericRepositoryAsync<tbl_Claims>
    {
        public ClaimRepository(_DbContext context, InstanceContext instance)
            : base(context, instance) { }

        public override async ValueTask<tbl_Claims> UpdateAsync(tbl_Claims model)
        {
            var entity = _context.Set<tbl_Claims>().Where(x => x.Id == model.Id).Single();

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

            return await Task.FromResult(_context.Update(entity).Entity);
        }
    }
}
