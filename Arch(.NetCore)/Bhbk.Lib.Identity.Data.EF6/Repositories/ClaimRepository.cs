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
    public class ClaimRepository : GenericRepository<uvw_Claims>
    {
        public ClaimRepository(IdentityEntities context, InstanceContext instance)
           : base(context, instance) { }

        public override uvw_Claims Create(uvw_Claims entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = (object)entity.Subject ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = entity.Type });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.Value });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.NVarChar) { Value = (object)entity.ValueType ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Claims>("[svc].[usp_Claim_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Claims> Create(IEnumerable<uvw_Claims> entities)
        {
            var results = new List<uvw_Claims>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Claims Delete(uvw_Claims entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Claims>("[svc].[usp_Claim_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Claims> Delete(IEnumerable<uvw_Claims> entities)
        {
            var results = new List<uvw_Claims>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Claims> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Claims Update(uvw_Claims entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = (object)entity.Subject ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.Type });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.NVarChar) { Value = entity.Value });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.NVarChar) { Value = (object)entity.ValueType ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Claims>("[svc].[usp_Claim_Update]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Claims> Update(IEnumerable<uvw_Claims> entities)
        {
            var results = new List<uvw_Claims>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
