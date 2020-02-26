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
    public class AudienceRepository : GenericRepository<uvw_Audiences>
    {
        public AudienceRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override uvw_Audiences Create(uvw_Audiences entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = entity.Name });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = (object)entity.ConcurrencyStamp ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.NVarChar) { Value = (object)entity.PasswordHash ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.NVarChar) { Value = (object)entity.SecurityStamp ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.NVarChar) { Value = entity.AudienceType });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p10", SqlDbType.Bit) { Value = entity.LockoutEnabled });
            pvalues.Add(new SqlParameter("@p11", SqlDbType.DateTimeOffset) { Value = entity.LockoutEnd.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p12", SqlDbType.DateTime2) { Value = entity.LastLoginSuccess.HasValue ? (object)entity.LastLoginSuccess.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p13", SqlDbType.DateTime2) { Value = entity.LastLoginFailure.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p14", SqlDbType.Int) { Value = entity.AccessFailedCount });
            pvalues.Add(new SqlParameter("@p15", SqlDbType.Int) { Value = entity.AccessSuccessCount });
            pvalues.Add(new SqlParameter("@p16", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Audiences>("[svc].[usp_Audience_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16", pvalues.ToArray()).Single();
        }

        public override uvw_Audiences Delete(uvw_Audiences entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Audiences>("[svc].[usp_Audience_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }
    }
}
