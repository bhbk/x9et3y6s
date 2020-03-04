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
    public class ClaimRepository : GenericRepository<uvw_Claims>
    {
        public ClaimRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Claims Create(uvw_Claims entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("@Type", SqlDbType.NVarChar) { Value = entity.Type },
                new SqlParameter("@Value", SqlDbType.NVarChar) { Value = entity.Value },
                new SqlParameter("@ValueType", SqlDbType.NVarChar) { Value = entity.ValueType },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Claims>().FromSqlRaw("[svc].[usp_Claim_Insert]"
                + "@IssuerId, @ActorId, @Subject, @Type, @Value, @ValueType, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Claim_Create]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Claims>().AsEnumerable().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Claims>().FromSqlRaw("[svc].[usp_Claim_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("@Type", SqlDbType.NVarChar) { Value = entity.Type },
                new SqlParameter("@Value", SqlDbType.NVarChar) { Value = entity.Value },
                new SqlParameter("@ValueType", SqlDbType.NVarChar) { Value = entity.ValueType },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Claims>().FromSqlRaw("[svc].[usp_Claim_Update]"
                + "@Id, @IssuerId, @ActorId, @Subject, @Type, @Value, @ValueType, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
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
