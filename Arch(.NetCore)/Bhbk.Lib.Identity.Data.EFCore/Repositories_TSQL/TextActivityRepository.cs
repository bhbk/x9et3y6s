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
    public class TextActivityRepository : GenericRepository<uvw_TextActivity>
    {
        public TextActivityRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_TextActivity Create(uvw_TextActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("EmailId", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("TwilioSid", SqlDbType.NVarChar) { Value = (object)entity.TwilioSid ?? DBNull.Value },
                new SqlParameter("TwilioStatus", SqlDbType.NVarChar) { Value = (object)entity.TwilioStatus ?? DBNull.Value },
                new SqlParameter("TwilioMessage", SqlDbType.NVarChar) { Value = (object)entity.TwilioMessage ?? DBNull.Value },
                new SqlParameter("StatusAtUtc", SqlDbType.DateTimeOffset) { Value = entity.StatusAtUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_TextActivity>("EXEC @ReturnValue = [svc].[usp_TextActivity_Insert]"
                + "@TextId, @TwilioSid, @TwilioStatus, @TwilioMessage, @StatusAtUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_TextActivity> Create(IEnumerable<uvw_TextActivity> entities)
        {
            var results = new List<uvw_TextActivity>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_TextActivity Delete(uvw_TextActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_TextActivity>("EXEC @ReturnValue = [svc].[usp_TextQueue_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_TextActivity> Delete(IEnumerable<uvw_TextActivity> entities)
        {
            var results = new List<uvw_TextActivity>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_TextActivity> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_TextActivity>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_TextActivity Update(uvw_TextActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("TextId", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("TwilioSid", SqlDbType.NVarChar) { Value = (object)entity.TwilioSid ?? DBNull.Value },
                new SqlParameter("TwilioStatus", SqlDbType.NVarChar) { Value = (object)entity.TwilioStatus ?? DBNull.Value },
                new SqlParameter("TwilioMessage", SqlDbType.NVarChar) { Value = (object)entity.TwilioMessage ?? DBNull.Value },
                new SqlParameter("StatusAtUtc", SqlDbType.DateTimeOffset) { Value = entity.StatusAtUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_TextActivity>("EXEC @ReturnValue = [svc].[usp_TextActivity_Update]"
                + "@Id, @TextId, @TwilioSid, @TwilioStatus, @TwilioMessage, @StatusAtUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_TextActivity> Update(IEnumerable<uvw_TextActivity> entities)
        {
            var results = new List<uvw_TextActivity>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
