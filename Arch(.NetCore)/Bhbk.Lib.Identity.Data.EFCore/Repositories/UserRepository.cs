﻿using Bhbk.Lib.Common.Services;
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
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : GenericRepository<uvw_User>
    {
        private IClockService _clock;

        public UserRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public override uvw_User Create(uvw_User entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@EmailAddress", SqlDbType.NVarChar) { Value = (object)entity.EmailAddress ?? DBNull.Value },
                new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = entity.FirstName },
                new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = entity.LastName },
                new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { Value = entity.PhoneNumber },
                new SqlParameter("@IsHumanBeing", SqlDbType.Bit) { Value = entity.IsHumanBeing },
                new SqlParameter("@IsLockedOut", SqlDbType.Bit) { Value = entity.IsLockedOut },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                new SqlParameter("@LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = entity.LockoutEndUtc.HasValue ? (object)entity.LockoutEndUtc.Value : DBNull.Value },
            };

            return _context.Set<uvw_User>().FromSqlRaw("[svc].[usp_User_Insert]"
                + "@ActorId, @UserName, @EmailAddress, @FirstName, @LastName, @PhoneNumber, @IsHumanBeing, @IsLockedOut, @IsDeletable, @LockoutEndUtc", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_User_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_User>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_User> Create(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_User Delete(uvw_User entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_User>().FromSqlRaw("[svc].[usp_User_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_User> Delete(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_User Update(uvw_User entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@EmailAddress", SqlDbType.NVarChar) { Value = (object)entity.EmailAddress ?? DBNull.Value },
                new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = entity.FirstName },
                new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = entity.LastName },
                new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { Value = entity.PhoneNumber },
                new SqlParameter("@IsHumanBeing", SqlDbType.Bit) { Value = entity.IsHumanBeing },
                new SqlParameter("@IsLockedOut", SqlDbType.Bit) { Value = entity.IsLockedOut },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                new SqlParameter("@LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = entity.LockoutEndUtc.HasValue ? (object)entity.LockoutEndUtc.Value : DBNull.Value },
            };

            return _context.Set<uvw_User>().FromSqlRaw("[svc].[usp_User_Update]"
                + "@Id, @ActorId, @UserName, @EmailAddress, @FirstName, @LastName, @PhoneNumber, @IsHumanBeing, @IsLockedOut, @IsDeletable, @LockoutEndUtc", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_User> Update(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}