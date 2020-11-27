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
    public class UrlRepository : GenericRepository<uvw_Url>
    {
        public UrlRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Url Create(uvw_Url entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("UrlHost", SqlDbType.NVarChar) { Value = (object)entity.UrlHost ?? DBNull.Value },
                new SqlParameter("UrlPath", SqlDbType.NVarChar) { Value = (object)entity.UrlPath ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Url>("EXEC @ReturnValue = [svc].[usp_Url_Insert] "
                + "@AudienceId, @UrlHost, @UrlPath, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Url> Create(IEnumerable<uvw_Url> entities)
        {
            var results = new List<uvw_Url>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Url Delete(uvw_Url entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Url>("EXEC @ReturnValue = [svc].[usp_Url_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Url> Delete(IEnumerable<uvw_Url> entities)
        {
            var results = new List<uvw_Url>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Url> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Url>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Url Update(uvw_Url entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("UrlHost", SqlDbType.NVarChar) { Value = (object)entity.UrlHost ?? DBNull.Value },
                new SqlParameter("UrlPath", SqlDbType.NVarChar) { Value = (object)entity.UrlPath ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Url>("EXEC @ReturnValue = [svc].[usp_Url_Update] "
                + "@Id, @AudienceId, @UrlHost, @UrlPath, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Url> Update(IEnumerable<uvw_Url> entities)
        {
            var results = new List<uvw_Url>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
