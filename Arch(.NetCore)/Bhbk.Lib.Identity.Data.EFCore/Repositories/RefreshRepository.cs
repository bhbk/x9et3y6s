using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class RefreshRepository : GenericRepository<uvw_Refreshes>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Refreshes Create(uvw_Refreshes entity)
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

            return _context.Set<uvw_Refreshes>().FromSqlRaw("[svc].[usp_Refresh_Insert]"
                + "@IssuerId, @AudienceId, @UserId, @RefreshValue, @RefreshType, @IssuerUtc, @ValueFromUtc, @ValueToUtc", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Refresh_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Refreshes>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Refreshes> Create(IEnumerable<uvw_Refreshes> entities)
        {
            var results = new List<uvw_Refreshes>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Refreshes Delete(uvw_Refreshes entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Refreshes>().FromSqlRaw("[svc].[usp_Refresh_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Refreshes> Delete(IEnumerable<uvw_Refreshes> entities)
        {
            var results = new List<uvw_Refreshes>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Refreshes> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Refreshes Update(uvw_Refreshes entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Refreshes> Update(IEnumerable<uvw_Refreshes> entities)
        {
            throw new NotImplementedException();
        }
    }
}
