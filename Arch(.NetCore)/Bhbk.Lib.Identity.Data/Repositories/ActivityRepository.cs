using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class ActivityRepository : GenericRepositoryAsync<tbl_Activities>
    {
        public ActivityRepository(_DbContext context, InstanceContext instance)
            : base(context, instance) { }

        public override ValueTask<tbl_Activities> UpdateAsync(tbl_Activities entity)
        {
            throw new NotImplementedException();
        }
    }
}
