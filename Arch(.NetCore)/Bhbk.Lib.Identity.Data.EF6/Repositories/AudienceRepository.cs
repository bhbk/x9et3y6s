using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

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
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.AudienceType });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.Bit) { Value = entity.LockoutEnabled });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.DateTimeOffset) { Value = entity.LockoutEnd.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.DateTime2) { Value = entity.LastLoginSuccess.HasValue ? (object)entity.LastLoginSuccess.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p10", SqlDbType.DateTime2) { Value = entity.LastLoginFailure.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p11", SqlDbType.Int) { Value = entity.AccessFailedCount });
            pvalues.Add(new SqlParameter("@p12", SqlDbType.Int) { Value = entity.AccessSuccessCount });
            pvalues.Add(new SqlParameter("@p13", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Audiences>("[svc].[usp_Audience_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Audiences> Create(IEnumerable<uvw_Audiences> entities)
        {
            var results = new List<uvw_Audiences>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Audiences Delete(uvw_Audiences entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Audiences>("[svc].[usp_Audience_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Audiences> Delete(IEnumerable<uvw_Audiences> entities)
        {
            var results = new List<uvw_Audiences>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Audiences> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Audiences Update(uvw_Audiences entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = entity.Name });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.NVarChar) { Value = entity.AudienceType });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.Bit) { Value = entity.LockoutEnabled });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.DateTimeOffset) { Value = entity.LockoutEnd.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p10", SqlDbType.DateTime2) { Value = entity.LastLoginSuccess.HasValue ? (object)entity.LastLoginSuccess.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p11", SqlDbType.DateTime2) { Value = entity.LastLoginFailure.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p12", SqlDbType.Int) { Value = entity.AccessFailedCount });
            pvalues.Add(new SqlParameter("@p13", SqlDbType.Int) { Value = entity.AccessSuccessCount });
            pvalues.Add(new SqlParameter("@p14", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Audiences>("[svc].[usp_Audience_Update]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Audiences> Update(IEnumerable<uvw_Audiences> entities)
        {
            var results = new List<uvw_Audiences>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
