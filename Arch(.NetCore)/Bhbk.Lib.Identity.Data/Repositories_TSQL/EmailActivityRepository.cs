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
    public class EmailActivityRepository : GenericRepository<uvw_EmailActivity>
    {
        public EmailActivityRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_EmailActivity Create(uvw_EmailActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("EmailId", SqlDbType.UniqueIdentifier) { Value = entity.EmailId },
                new SqlParameter("SendgridId", SqlDbType.NVarChar) { Value = (object)entity.SendgridId ?? DBNull.Value },
                new SqlParameter("SendgridStatus", SqlDbType.NVarChar) { Value = (object)entity.SendgridStatus ?? DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailActivity>("EXEC @ReturnValue = [svc].[usp_EmailActivity_Insert] "
                + "@EmailId, @SendgridId, @SendgridStatus", pvalues).Single();
        }

        public override IEnumerable<uvw_EmailActivity> Create(IEnumerable<uvw_EmailActivity> entities)
        {
            var results = new List<uvw_EmailActivity>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_EmailActivity Delete(uvw_EmailActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailActivity>("EXEC @ReturnValue = [svc].[usp_EmailQueue_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_EmailActivity> Delete(IEnumerable<uvw_EmailActivity> entities)
        {
            var results = new List<uvw_EmailActivity>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_EmailActivity> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_EmailActivity>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_EmailActivity Update(uvw_EmailActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("EmailId", SqlDbType.UniqueIdentifier) { Value = entity.EmailId },
                new SqlParameter("SendgridId", SqlDbType.NVarChar) { Value = (object)entity.SendgridId ?? DBNull.Value },
                new SqlParameter("SendgridStatus", SqlDbType.NVarChar) { Value = (object)entity.SendgridStatus ?? DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailActivity>("EXEC @ReturnValue = [svc].[usp_EmailActivity_Update] "
                + "@Id, @EmailId, @SendgridId, @SendgridStatus", pvalues).Single();
        }

        public override IEnumerable<uvw_EmailActivity> Update(IEnumerable<uvw_EmailActivity> entities)
        {
            var results = new List<uvw_EmailActivity>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
