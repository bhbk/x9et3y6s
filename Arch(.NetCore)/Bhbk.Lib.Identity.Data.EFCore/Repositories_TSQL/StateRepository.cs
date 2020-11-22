using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_TSQL;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_TSQL
{
    public class StateRepository : GenericRepository<uvw_State>
    {
        public StateRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_State Create(uvw_State entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("StateValue", SqlDbType.NVarChar) { Value = (object)entity.StateValue ?? DBNull.Value },
                new SqlParameter("StateType", SqlDbType.NVarChar) { Value = entity.StateType },
                new SqlParameter("StateDecision", SqlDbType.Bit) { Value = entity.StateDecision.HasValue ? (object)entity.StateDecision.Value : DBNull.Value },
                new SqlParameter("StateConsume", SqlDbType.Bit) { Value = entity.StateConsume },
                new SqlParameter("IssuerUtc", SqlDbType.DateTimeOffset) { Value = entity.IssuedUtc },
                new SqlParameter("ValidFromUtc", SqlDbType.DateTimeOffset) { Value = entity.ValidFromUtc },
                new SqlParameter("ValidToUtc", SqlDbType.DateTimeOffset) { Value = entity.ValidToUtc },
                new SqlParameter("LastPollingUtc", SqlDbType.DateTimeOffset) { Value = entity.LastPollingUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_State>("EXEC @ReturnValue = [svc].[usp_State_Insert]"
                + "@IssuerId, @AudienceId, @UserId, @StateValue, @StateType, @StateDecision, @StateConsume, @IssuerUtc, @ValidFromUtc, @ValidToUtc, @LastPollingUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_State> Create(IEnumerable<uvw_State> entities)
        {
            var results = new List<uvw_State>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_State Delete(uvw_State entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_State>("EXEC @ReturnValue = [svc].[usp_State_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_State> Delete(IEnumerable<uvw_State> entities)
        {
            var results = new List<uvw_State>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_State> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_State>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_State Update(uvw_State entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("StateValue", SqlDbType.NVarChar) { Value = (object)entity.StateValue ?? DBNull.Value },
                new SqlParameter("StateType", SqlDbType.NVarChar) { Value = entity.StateType },
                new SqlParameter("StateDecision", SqlDbType.Bit) { Value = entity.StateDecision.HasValue ? (object)entity.StateDecision.Value : DBNull.Value },
                new SqlParameter("StateConsume", SqlDbType.Bit) { Value = entity.StateConsume },
                new SqlParameter("IssuerUtc", SqlDbType.DateTimeOffset) { Value = entity.IssuedUtc },
                new SqlParameter("ValidFromUtc", SqlDbType.DateTimeOffset) { Value = entity.ValidFromUtc },
                new SqlParameter("ValidToUtc", SqlDbType.DateTimeOffset) { Value = entity.ValidToUtc },
                new SqlParameter("LastPollingUtc", SqlDbType.DateTimeOffset) { Value = entity.LastPollingUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_State>("EXEC @ReturnValue = [svc].[usp_State_Update]"
                + "@Id, @IssuerId, @AudienceId, @UserId, @StateValue, @StateType, @StateDecision, @StateConsume, @IssuerUtc, @ValidFromUtc, @ValidToUtc, @LastPollingUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_State> Update(IEnumerable<uvw_State> entities)
        {
            var results = new List<uvw_State>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
