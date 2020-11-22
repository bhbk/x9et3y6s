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
    public class EmailQueueRepository : GenericRepository<uvw_EmailQueue>
    {
        public EmailQueueRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_EmailQueue Create(uvw_EmailQueue entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("FromId", SqlDbType.UniqueIdentifier) { Value = entity.FromId.HasValue ? (object)entity.FromId.Value : DBNull.Value },
                new SqlParameter("FromEmail", SqlDbType.NVarChar) { Value = entity.FromEmail },
                new SqlParameter("FromDisplay", SqlDbType.NVarChar) { Value = (object)entity.FromDisplay ?? DBNull.Value },
                new SqlParameter("ToId", SqlDbType.UniqueIdentifier) { Value = entity.ToId.HasValue ? (object)entity.ToId.Value : DBNull.Value },
                new SqlParameter("ToEmail", SqlDbType.NVarChar) { Value = entity.ToEmail },
                new SqlParameter("ToDisplay", SqlDbType.NVarChar) { Value = (object)entity.ToDisplay ?? DBNull.Value },
                new SqlParameter("Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("Body", SqlDbType.NVarChar) { Value = (object)entity.Body ?? DBNull.Value },
                new SqlParameter("SentAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailQueue>("EXEC @ReturnValue = [svc].[usp_EmailQueue_Insert]"
                + "@FromId, @FromEmail, @FromDisplay, @ToId, @ToEmail, @ToDisplay, @Subject, @Body, @SentAtUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_EmailQueue> Create(IEnumerable<uvw_EmailQueue> entities)
        {
            var results = new List<uvw_EmailQueue>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_EmailQueue Delete(uvw_EmailQueue entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailQueue>("EXEC @ReturnValue = [svc].[usp_EmailQueue_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_EmailQueue> Delete(IEnumerable<uvw_EmailQueue> entities)
        {
            var results = new List<uvw_EmailQueue>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_EmailQueue> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_EmailQueue>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_EmailQueue Update(uvw_EmailQueue entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("FromId", SqlDbType.UniqueIdentifier) { Value = entity.FromId.HasValue ? (object)entity.FromId.Value : DBNull.Value },
                new SqlParameter("FromEmail", SqlDbType.NVarChar) { Value = entity.FromEmail },
                new SqlParameter("FromDisplay", SqlDbType.NVarChar) { Value = (object)entity.FromDisplay ?? DBNull.Value },
                new SqlParameter("ToId", SqlDbType.UniqueIdentifier) { Value = entity.ToId.HasValue ? (object)entity.ToId.Value : DBNull.Value },
                new SqlParameter("ToEmail", SqlDbType.NVarChar) { Value = entity.ToEmail },
                new SqlParameter("ToDisplay", SqlDbType.NVarChar) { Value = (object)entity.ToDisplay ?? DBNull.Value },
                new SqlParameter("Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("Body", SqlDbType.NVarChar) { Value = (object)entity.Body ?? DBNull.Value },
                new SqlParameter("SentAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailQueue>("EXEC @ReturnValue = [svc].[usp_EmailQueue_Update]"
                + "@Id, @FromId, @FromEmail, @FromDisplay, @ToId, @ToEmail, @ToDisplay, @Subject, @Body, @SentAtUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_EmailQueue> Update(IEnumerable<uvw_EmailQueue> entities)
        {
            var results = new List<uvw_EmailQueue>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
