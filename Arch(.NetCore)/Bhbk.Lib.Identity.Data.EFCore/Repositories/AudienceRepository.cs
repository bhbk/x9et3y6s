using Bhbk.Lib.Common.Services;
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
    public class AudienceRepository : GenericRepository<uvw_Audience>
    {
        private IClockService _clock;

        public AudienceRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public override uvw_Audience Create(uvw_Audience entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@IsLockedOut", SqlDbType.Bit) { Value = entity.IsLockedOut },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                new SqlParameter("@AccessFailedCount", SqlDbType.Int) { Value = entity.AccessFailedCount },
                new SqlParameter("@AccessSuccessCount", SqlDbType.Int) { Value = entity.AccessSuccessCount },
                new SqlParameter("@LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = entity.LockoutEndUtc.HasValue ? (object)entity.LockoutEndUtc.Value : DBNull.Value },
                new SqlParameter("@LastLoginSuccessUtc", SqlDbType.DateTimeOffset) { Value = entity.LastLoginSuccessUtc.HasValue ? (object)entity.LastLoginSuccessUtc.Value : DBNull.Value },
                new SqlParameter("@LastLoginFailureUtc", SqlDbType.DateTimeOffset) { Value = entity.LastLoginFailureUtc.HasValue ? (object)entity.LockoutEndUtc.Value : DBNull.Value },
            };

            return _context.Set<uvw_Audience>().FromSqlRaw("[svc].[usp_Audience_Insert]"
                + "@IssuerId, @ActorId, @Name, @Description, @IsLockedOut, @IsEnabled, @IsDeletable, "
                + "@AccessFailedCount, @AccessSuccessCount, @LockoutEndUtc, @LastLoginSuccessUtc, @LastLoginFailureUtc", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Audience_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Audience>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Audience> Create(IEnumerable<uvw_Audience> entities)
        {
            var results = new List<uvw_Audience>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Audience Delete(uvw_Audience entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Audience>().FromSqlRaw("[svc].[usp_Audience_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Audience> Delete(IEnumerable<uvw_Audience> entities)
        {
            var results = new List<uvw_Audience>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Audience Update(uvw_Audience entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@IsLockedOut", SqlDbType.Bit) { Value = entity.IsLockedOut },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                new SqlParameter("@AccessFailedCount", SqlDbType.Int) { Value = entity.AccessFailedCount },
                new SqlParameter("@AccessSuccessCount", SqlDbType.Int) { Value = entity.AccessSuccessCount },
                new SqlParameter("@LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = entity.LockoutEndUtc.HasValue ? (object)entity.LockoutEndUtc.Value : DBNull.Value },
                new SqlParameter("@LastLoginSuccessUtc", SqlDbType.DateTimeOffset) { Value = entity.LastLoginSuccessUtc.HasValue ? (object)entity.LastLoginSuccessUtc.Value : DBNull.Value },
                new SqlParameter("@LastLoginFailureUtc", SqlDbType.DateTimeOffset) { Value = entity.LastLoginFailureUtc.HasValue ? (object)entity.LockoutEndUtc.Value : DBNull.Value },
            };

            return _context.Set<uvw_Audience>().FromSqlRaw("[svc].[usp_Audience_Update]"
                + "@Id, @IssuerId, @ActorId, @Name, @Description, @IsLockedOut, @IsEnabled, @IsDeletable, "
                + "@AccessFailedCount, @AccessSuccessCount, @LockoutEndUtc, @LastLoginSuccessUtc, @LastLoginFailureUtc", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Audience> Update(IEnumerable<uvw_Audience> entities)
        {
            var results = new List<uvw_Audience>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
