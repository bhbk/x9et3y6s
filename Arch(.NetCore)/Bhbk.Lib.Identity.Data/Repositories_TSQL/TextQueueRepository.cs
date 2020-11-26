using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models_TSQL;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.Repositories_TSQL
{
    public class TextQueueRepository : GenericRepository<uvw_TextQueue>
    {
        public TextQueueRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_TextQueue Create(uvw_TextQueue entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("FromPhoneNumber", SqlDbType.NVarChar) { Value = entity.FromPhoneNumber },
                new SqlParameter("ToPhoneNumber", SqlDbType.NVarChar) { Value = entity.ToPhoneNumber },
                new SqlParameter("Body", SqlDbType.NVarChar) { Value = entity.Body },
                new SqlParameter("SendAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_TextQueue>("EXEC @ReturnValue = [svc].[usp_TextQueue_Insert] "
                + "@FromPhoneNumber, @ToPhoneNumber, @Body, @SendAtUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_TextQueue> Create(IEnumerable<uvw_TextQueue> entities)
        {
            var results = new List<uvw_TextQueue>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_TextQueue Delete(uvw_TextQueue entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_TextQueue>("EXEC @ReturnValue = [svc].[usp_TextQueue_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_TextQueue> Delete(IEnumerable<uvw_TextQueue> entities)
        {
            var results = new List<uvw_TextQueue>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_TextQueue> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_TextQueue>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_TextQueue Update(uvw_TextQueue entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("IsCancelled", SqlDbType.Bit) { Value = entity.IsCancelled },
                new SqlParameter("SendAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
                new SqlParameter("DeliveredUtc", SqlDbType.DateTimeOffset) { Value = entity.DeliveredUtc.HasValue ? (object)entity.DeliveredUtc.Value : DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_TextQueue>("EXEC @ReturnValue = [svc].[usp_TextQueue_Update] "
                + "@Id, @IsCancelled, @SendAtUtc, @DeliveredUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_TextQueue> Update(IEnumerable<uvw_TextQueue> entities)
        {
            var results = new List<uvw_TextQueue>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
