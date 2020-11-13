﻿using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class MOTDRepository : GenericRepository<uvw_MOTD>
    {
        public MOTDRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_MOTD Create(uvw_MOTD entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Author", SqlDbType.NVarChar) { Value = entity.Author },
                new SqlParameter("@Quote", SqlDbType.NVarChar) { Value = entity.Quote },
                new SqlParameter("@TssId", SqlDbType.NVarChar) { Value = (object)entity.TssId ?? DBNull.Value },
                new SqlParameter("@TssTitle", SqlDbType.NVarChar) { Value = (object)entity.TssTitle ?? DBNull.Value },
                new SqlParameter("@TssCategory", SqlDbType.NVarChar) { Value = (object)entity.TssCategory ?? DBNull.Value },
                new SqlParameter("@TssDate", SqlDbType.DateTime) { Value = entity.TssDate.HasValue ? (object)entity.TssDate.Value : DBNull.Value },
                new SqlParameter("@TssTags", SqlDbType.NVarChar) { Value = (object)entity.TssTags ?? DBNull.Value },
                new SqlParameter("@TssLength", SqlDbType.Int) { Value = entity.TssLength.HasValue ? (object)entity.TssLength.Value : DBNull.Value },
                new SqlParameter("@TssBackground", SqlDbType.NVarChar) { Value = (object)entity.Author ?? DBNull.Value },
            };

            return _context.Set<uvw_MOTD>().FromSqlRaw("[svc].[usp_MOTD_Insert]"
                + "@Author, @Quote, @TssId, @TssTitle, @TssCategory, @TssDate, @TssTags, @TssLength, @TssBackground", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_MOTD_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_MOTD>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_MOTD> Create(IEnumerable<uvw_MOTD> entities)
        {
            var results = new List<uvw_MOTD>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_MOTD Delete(uvw_MOTD entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_MOTD>().FromSqlRaw("[svc].[usp_MOTD_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_MOTD> Delete(IEnumerable<uvw_MOTD> entities)
        {
            var results = new List<uvw_MOTD>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_MOTD Update(uvw_MOTD entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@Author", SqlDbType.NVarChar) { Value = entity.Author },
                new SqlParameter("@Quote", SqlDbType.NVarChar) { Value = entity.Quote },
                new SqlParameter("@TssId", SqlDbType.NVarChar) { Value = (object)entity.TssId ?? DBNull.Value },
                new SqlParameter("@TssTitle", SqlDbType.NVarChar) { Value = (object)entity.TssTitle ?? DBNull.Value },
                new SqlParameter("@TssCategory", SqlDbType.NVarChar) { Value = (object)entity.TssCategory ?? DBNull.Value },
                new SqlParameter("@TssDate", SqlDbType.DateTime) { Value = entity.TssDate.HasValue ? (object)entity.TssDate.Value : DBNull.Value },
                new SqlParameter("@TssTags", SqlDbType.NVarChar) { Value = (object)entity.TssTags ?? DBNull.Value },
                new SqlParameter("@TssLength", SqlDbType.Int) { Value = entity.TssLength.HasValue ? (object)entity.TssLength.Value : DBNull.Value },
                new SqlParameter("@TssBackground", SqlDbType.NVarChar) { Value = (object)entity.Author ?? DBNull.Value },
            };

            return _context.Set<uvw_MOTD>().FromSqlRaw("[svc].[usp_MOTD_Update]"
                + "@Id, @Author, @Quote, @TssId, @TssTitle, @TssCategory, @TssDate, @TssTags, @TssLength, @TssBackground", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_MOTD> Update(IEnumerable<uvw_MOTD> entities)
        {
            var results = new List<uvw_MOTD>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
