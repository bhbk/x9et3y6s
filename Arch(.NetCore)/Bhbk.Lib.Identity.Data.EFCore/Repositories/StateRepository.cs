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
    public class StateRepository : GenericRepository<uvw_States>
    {
        public StateRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_States Create(uvw_States entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("@StateValue", SqlDbType.NVarChar) { Value = (object)entity.StateValue ?? DBNull.Value },
                new SqlParameter("@StateType", SqlDbType.NVarChar) { Value = entity.StateType },
                new SqlParameter("@StateDecision", SqlDbType.Bit) { Value = entity.StateDecision.HasValue ? (object)entity.StateDecision.Value : DBNull.Value },
                new SqlParameter("@StateConsume", SqlDbType.Bit) { Value = entity.StateConsume },
                new SqlParameter("@IssuerUtc", SqlDbType.DateTime2) { Value = entity.IssuedUtc },
                new SqlParameter("@ValidFromUtc", SqlDbType.DateTime2) { Value = entity.ValidFromUtc },
                new SqlParameter("@ValidToUtc", SqlDbType.DateTime2) { Value = entity.ValidToUtc },
                new SqlParameter("@LastPolling", SqlDbType.DateTime2) { Value = entity.LastPolling }
            };

            return _context.Set<uvw_States>().FromSqlRaw("[svc].[usp_State_Insert]"
                + "@IssuerId, @AudienceId, @UserId, @StateValue, @StateType, @StateDecision, @StateConsume, @IssuerUtc, @ValidFromUtc, @ValidToUtc, @LastPolling", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_State_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_States>().AsEnumerable().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_States>().FromSqlRaw("[svc].[usp_State_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("@StateValue", SqlDbType.NVarChar) { Value = (object)entity.StateValue ?? DBNull.Value },
                new SqlParameter("@StateType", SqlDbType.NVarChar) { Value = entity.StateType },
                new SqlParameter("@StateDecision", SqlDbType.Bit) { Value = entity.StateDecision.HasValue ? (object)entity.StateDecision.Value : DBNull.Value },
                new SqlParameter("@StateConsume", SqlDbType.Bit) { Value = entity.StateConsume },
                new SqlParameter("@IssuerUtc", SqlDbType.DateTime2) { Value = entity.IssuedUtc },
                new SqlParameter("@ValidFromUtc", SqlDbType.DateTime2) { Value = entity.ValidFromUtc },
                new SqlParameter("@ValidToUtc", SqlDbType.DateTime2) { Value = entity.ValidToUtc },
                new SqlParameter("@LastPolling", SqlDbType.DateTime2) { Value = entity.LastPolling }
            };

            return _context.Set<uvw_States>().FromSqlRaw("[svc].[usp_State_Update]"
                + "@Id, @IssuerId, @AudienceId, @UserId, @StateValue, @StateType, @StateDecision, @StateConsume, @IssuerUtc, @ValidFromUtc, @ValidToUtc, @LastPolling", pvalues.ToArray())
                    .AsEnumerable().Single();
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
