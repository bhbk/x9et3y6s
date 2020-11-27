using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using System.Linq.Expressions;
namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class MOTDRepository : GenericRepository<uvw_MOTD>
    {
        public MOTDRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_MOTD Create(uvw_MOTD entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Author", SqlDbType.NVarChar) { Value = entity.Author },
                new SqlParameter("Quote", SqlDbType.NVarChar) { Value = entity.Quote },
                new SqlParameter("TssId", SqlDbType.NVarChar) { Value = (object)entity.TssId ?? DBNull.Value },
                new SqlParameter("TssTitle", SqlDbType.NVarChar) { Value = (object)entity.TssTitle ?? DBNull.Value },
                new SqlParameter("TssCategory", SqlDbType.NVarChar) { Value = (object)entity.TssCategory ?? DBNull.Value },
                new SqlParameter("TssDate", SqlDbType.DateTime) { Value = entity.TssDate.HasValue ? (object)entity.TssDate.Value : DBNull.Value },
                new SqlParameter("TssTags", SqlDbType.NVarChar) { Value = (object)entity.TssTags ?? DBNull.Value },
                new SqlParameter("TssLength", SqlDbType.Int) { Value = entity.TssLength.HasValue ? (object)entity.TssLength.Value : DBNull.Value },
                new SqlParameter("TssBackground", SqlDbType.NVarChar) { Value = (object)entity.Author ?? DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_MOTD>("EXEC @ReturnValue = [svc].[usp_MOTD_Insert] "
                + "@Author, @Quote, @TssId, @TssTitle, @TssCategory, @TssDate, @TssTags, @TssLength, @TssBackground", pvalues).Single();
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
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_MOTD>("EXEC @ReturnValue = [svc].[usp_MOTD_Delete] @Id", pvalues).Single();
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

        public override IEnumerable<uvw_MOTD> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_MOTD>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_MOTD Update(uvw_MOTD entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("Author", SqlDbType.NVarChar) { Value = entity.Author },
                new SqlParameter("Quote", SqlDbType.NVarChar) { Value = entity.Quote },
                new SqlParameter("TssId", SqlDbType.NVarChar) { Value = (object)entity.TssId ?? DBNull.Value },
                new SqlParameter("TssTitle", SqlDbType.NVarChar) { Value = (object)entity.TssTitle ?? DBNull.Value },
                new SqlParameter("TssCategory", SqlDbType.NVarChar) { Value = (object)entity.TssCategory ?? DBNull.Value },
                new SqlParameter("TssDate", SqlDbType.DateTime) { Value = entity.TssDate.HasValue ? (object)entity.TssDate.Value : DBNull.Value },
                new SqlParameter("TssTags", SqlDbType.NVarChar) { Value = (object)entity.TssTags ?? DBNull.Value },
                new SqlParameter("TssLength", SqlDbType.Int) { Value = entity.TssLength.HasValue ? (object)entity.TssLength.Value : DBNull.Value },
                new SqlParameter("TssBackground", SqlDbType.NVarChar) { Value = (object)entity.Author ?? DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_MOTD>("EXEC @ReturnValue = [svc].[usp_MOTD_Update] "
                + "@Id, @Author, @Quote, @TssId, @TssTitle, @TssCategory, @TssDate, @TssTags, @TssLength, @TssBackground", pvalues).Single();
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
