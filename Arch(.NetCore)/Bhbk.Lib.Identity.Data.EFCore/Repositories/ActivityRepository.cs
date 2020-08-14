using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class ActivityRepository : GenericRepository<uvw_Activity>
    {
        public ActivityRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Activity Create(uvw_Activity entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@AudienceID", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("@UserID", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("@ActivityType", SqlDbType.NVarChar) { Value = entity.ActivityType },
                new SqlParameter("@TableName", SqlDbType.NVarChar) { Value = (object)entity.TableName ?? DBNull.Value },
                new SqlParameter("@KeyValues", SqlDbType.NVarChar) { Value = (object)entity.KeyValues ?? DBNull.Value },
                new SqlParameter("@OriginalValues", SqlDbType.NVarChar) { Value = (object)entity.OriginalValues ?? DBNull.Value },
                new SqlParameter("@CurrentValues", SqlDbType.NVarChar) { Value = (object)entity.CurrentValues ?? DBNull.Value },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Activity>().FromSqlRaw("[svc].[usp_Activity_Insert]"
                + "@AudienceId, @UserId, @ActivityType, @TableName, @KeyValues, @OriginalValues, @CurrentValues, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Activity_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Activity>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Activity> Create(IEnumerable<uvw_Activity> entities)
        {
            var results = new List<uvw_Activity>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Activity Delete(uvw_Activity entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Activity>().FromSqlRaw("[svc].[usp_Activity_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Activity> Delete(IEnumerable<uvw_Activity> entities)
        {
            var results = new List<uvw_Activity>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Activity> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Activity Update(uvw_Activity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Activity> Update(IEnumerable<uvw_Activity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
