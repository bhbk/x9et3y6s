using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class RefreshRepository : GenericRepository<uvw_Refreshes>
    {
        public RefreshRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override uvw_Refreshes Create(uvw_Refreshes entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = entity.RefreshValue });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.RefreshType });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.DateTime2) { Value = entity.IssuedUtc });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.ValidFromUtc });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.ValidToUtc });

            return _context.Database.SqlQuery<uvw_Refreshes>("[svc].[usp_Refresh_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7", pvalues.ToArray()).Single();
        }

        public override uvw_Refreshes Delete(uvw_Refreshes entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Refreshes>("[svc].[usp_Refresh_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }
    }
}
