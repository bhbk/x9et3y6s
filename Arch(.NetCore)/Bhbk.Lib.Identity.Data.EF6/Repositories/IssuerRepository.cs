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
    public class IssuerRepository : GenericRepository<uvw_Issuers>
    {
        public IssuerRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override uvw_Issuers Create(uvw_Issuers entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.NVarChar) { Value = entity.Name });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = entity.IssuerKey });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.Bit) { Value = entity.Enabled });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Issuers>("[svc].[usp_Issuer_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7", pvalues.ToArray()).Single();
        }

        public override uvw_Issuers Delete(uvw_Issuers entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Issuers>("[svc].[usp_Issuer_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }
    }
}
