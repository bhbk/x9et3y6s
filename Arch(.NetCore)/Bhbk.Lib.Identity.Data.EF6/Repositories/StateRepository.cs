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
    public class StateRepository : GenericRepository<uvw_States>
    {
        public StateRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_States Create(uvw_States entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = string.IsNullOrEmpty(entity.StateValue) ? (object)entity.StateValue : DBNull.Value });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.StateType });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.Bit) { Value = entity.StateDecision.HasValue ? (object)entity.StateDecision.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.Bit) { Value = entity.StateConsume });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.IssuedUtc });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.DateTime2) { Value = entity.ValidToUtc });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.DateTime2) { Value = entity.ValidFromUtc });
            pvalues.Add(new SqlParameter("@p10", SqlDbType.DateTime2) { Value = entity.LastPolling });

            return _context.Database.SqlQuery<uvw_States>("[svc].[usp_State_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_States> Create(IEnumerable<uvw_States> entities)
        {
            var results = new List<uvw_States>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_States Delete(uvw_States entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_States>("[svc].[usp_State_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_States> Delete(IEnumerable<uvw_States> entities)
        {
            var results = new List<uvw_States>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_States> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_States Update(uvw_States entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = string.IsNullOrEmpty(entity.StateValue) ? (object)entity.StateValue : DBNull.Value });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.NVarChar) { Value = entity.StateType });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.Bit) { Value = entity.StateDecision.HasValue ? (object)entity.StateDecision.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.Bit) { Value = entity.StateConsume });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.DateTime2) { Value = entity.IssuedUtc });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.DateTime2) { Value = entity.ValidToUtc });
            pvalues.Add(new SqlParameter("@p10", SqlDbType.DateTime2) { Value = entity.ValidFromUtc });
            pvalues.Add(new SqlParameter("@p11", SqlDbType.DateTime2) { Value = entity.LastPolling });

            return _context.Database.SqlQuery<uvw_States>("[svc].[usp_State_Update]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p11", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_States> Update(IEnumerable<uvw_States> entities)
        {
            var results = new List<uvw_States>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
