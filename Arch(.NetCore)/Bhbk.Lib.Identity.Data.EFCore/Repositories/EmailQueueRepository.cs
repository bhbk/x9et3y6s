using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class EmailQueueRepository : GenericRepository<uvw_EmailQueue>
    {
        public EmailQueueRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_EmailQueue Create(uvw_EmailQueue entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@FromId", SqlDbType.UniqueIdentifier) { Value = entity.FromId.HasValue ? (object)entity.FromId.Value : DBNull.Value },
                new SqlParameter("@FromEmail", SqlDbType.NVarChar) { Value = entity.FromEmail },
                new SqlParameter("@FromDisplay", SqlDbType.NVarChar) { Value = (object)entity.FromDisplay ?? DBNull.Value },
                new SqlParameter("@ToId", SqlDbType.UniqueIdentifier) { Value = entity.ToId.HasValue ? (object)entity.ToId.Value : DBNull.Value },
                new SqlParameter("@ToEmail", SqlDbType.NVarChar) { Value = entity.ToEmail },
                new SqlParameter("@ToDisplay", SqlDbType.NVarChar) { Value = (object)entity.ToDisplay ?? DBNull.Value },
                new SqlParameter("@Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("@Body", SqlDbType.NVarChar) { Value = (object)entity.Body ?? DBNull.Value },
                new SqlParameter("@SentAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
            };

            return _context.Set<uvw_EmailQueue>().FromSqlRaw("[svc].[usp_EmailQueue_Insert]"
                + "@ActorId, @FromId, @FromEmail, @FromDisplay, @ToId, @ToEmail, @ToDisplay, @Subject, @Body, @SentAtUtc", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_EmailQueue_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_EmailQueue>().AsEnumerable().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_EmailQueue>().FromSqlRaw("[svc].[usp_EmailQueue_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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

        public override uvw_EmailQueue Update(uvw_EmailQueue entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@FromId", SqlDbType.UniqueIdentifier) { Value = entity.FromId.HasValue ? (object)entity.FromId.Value : DBNull.Value },
                new SqlParameter("@FromEmail", SqlDbType.NVarChar) { Value = entity.FromEmail },
                new SqlParameter("@FromDisplay", SqlDbType.NVarChar) { Value = (object)entity.FromDisplay ?? DBNull.Value },
                new SqlParameter("@ToId", SqlDbType.UniqueIdentifier) { Value = entity.ToId.HasValue ? (object)entity.ToId.Value : DBNull.Value },
                new SqlParameter("@ToEmail", SqlDbType.NVarChar) { Value = entity.ToEmail },
                new SqlParameter("@ToDisplay", SqlDbType.NVarChar) { Value = (object)entity.ToDisplay ?? DBNull.Value },
                new SqlParameter("@Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("@Body", SqlDbType.NVarChar) { Value = (object)entity.Body ?? DBNull.Value },
                new SqlParameter("@SentAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
            };

            return _context.Set<uvw_EmailQueue>().FromSqlRaw("[svc].[usp_EmailQueue_Update]"
                + "@Id, @ActorId, @FromId, @FromEmail, @FromDisplay, @ToId, @ToEmail, @ToDisplay, @Subject, @Body, @SentAtUtc", pvalues.ToArray())
                    .AsEnumerable().Single();
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
