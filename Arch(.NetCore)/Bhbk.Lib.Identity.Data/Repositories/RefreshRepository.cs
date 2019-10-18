using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class RefreshRepository : GenericRepositoryAsync<tbl_Refreshes>
    {
        public RefreshRepository(_DbContext context, InstanceContext instance)
            : base(context, instance) { }

        public override Task<tbl_Refreshes> UpdateAsync(tbl_Refreshes entity)
        {
            throw new NotImplementedException();
        }
    }
}
