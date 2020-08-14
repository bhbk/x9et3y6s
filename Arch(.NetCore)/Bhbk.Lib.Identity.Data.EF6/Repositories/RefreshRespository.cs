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
    public class RefreshRepository : GenericRepository<uvw_Refresh>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Refresh Create(uvw_Refresh entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("@RefreshValue", SqlDbType.NVarChar) { Value = entity.RefreshValue },
                new SqlParameter("@RefreshType", SqlDbType.NVarChar) { Value = entity.RefreshType },
                new SqlParameter("@IssuerUtc", SqlDbType.DateTime2) { Value = entity.IssuedUtc },
                new SqlParameter("@ValueFromUtc", SqlDbType.DateTime2) { Value = entity.ValidFromUtc },
                new SqlParameter("@ValueToUtc", SqlDbType.DateTime2) { Value = entity.ValidToUtc }
            };

            return _context.Database.SqlQuery<uvw_Refresh>("[svc].[usp_Refresh_Insert]"
                + "@IssuerId, @AudienceId, @UserId, @RefreshValue, @RefreshType, @IssuerUtc, @ValueFromUtc, @ValueToUtc", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Refresh_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Refresh>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Refresh> Create(IEnumerable<uvw_Refresh> entities)
        {
            var results = new List<uvw_Refresh>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Refresh Delete(uvw_Refresh entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Database.SqlQuery<uvw_Refresh>("[svc].[usp_Refresh_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Refresh> Delete(IEnumerable<uvw_Refresh> entities)
        {
            var results = new List<uvw_Refresh>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Refresh> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Refresh Update(uvw_Refresh entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Refresh> Update(IEnumerable<uvw_Refresh> entities)
        {
            throw new NotImplementedException();
        }
    }
}
