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
    public class TextQueueRepository : GenericRepository<uvw_TextQueue>
    {
        public TextQueueRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_TextQueue Create(uvw_TextQueue entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@FromId", SqlDbType.UniqueIdentifier) { Value = entity.FromId.HasValue ? (object)entity.FromId.Value : DBNull.Value },
                new SqlParameter("@FromPhoneNumber", SqlDbType.NVarChar) { Value = entity.FromPhoneNumber },
                new SqlParameter("@ToId", SqlDbType.UniqueIdentifier) { Value = entity.ToId.HasValue ? (object)entity.ToId.Value : DBNull.Value },
                new SqlParameter("@ToPhoneNumber", SqlDbType.NVarChar) { Value = entity.ToPhoneNumber },
                new SqlParameter("@Body", SqlDbType.NVarChar) { Value = entity.Body },
                new SqlParameter("@SentAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
            };

            return _context.Set<uvw_TextQueue>().FromSqlRaw("[svc].[usp_TextQueue_Insert]"
                + "@ActorId, @FromId, @FromPhoneNumber, @ToId, @ToPhoneNumber, @Body, @SentAtUtc", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_TextQueue_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_TextQueue>().AsEnumerable().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_TextQueue>().FromSqlRaw("[svc].[usp_TextQueue_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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

        public override uvw_TextQueue Update(uvw_TextQueue entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@FromId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@FromPhoneNumber", SqlDbType.NVarChar) { Value = (object)entity.FromPhoneNumber ?? DBNull.Value },
                new SqlParameter("@ToId", SqlDbType.UniqueIdentifier) { Value = entity.ToId },
                new SqlParameter("@ToPhoneNumber", SqlDbType.NVarChar) { Value = entity.ToPhoneNumber },
                new SqlParameter("@Body", SqlDbType.NVarChar) { Value = entity.Body },
                new SqlParameter("@SentAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
            };

            return _context.Set<uvw_TextQueue>().FromSqlRaw("[svc].[usp_TextQueue_Update]"
                + "@Id, @ActorId, @FromId, @FromPhoneNumber, @ToId, @ToPhoneNumber, @Body, @SentAtUtc", pvalues.ToArray())
                    .AsEnumerable().Single();
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
