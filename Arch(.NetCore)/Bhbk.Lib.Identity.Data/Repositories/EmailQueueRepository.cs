﻿using Bhbk.Lib.DataAccess.EFCore.Extensions;
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
    public class EmailQueueRepository : GenericRepository<uvw_EmailQueue>
    {
        public EmailQueueRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_EmailQueue Create(uvw_EmailQueue entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("FromEmail", SqlDbType.NVarChar) { Value = entity.FromEmail },
                new SqlParameter("FromDisplay", SqlDbType.NVarChar) { Value = (object)entity.FromDisplay ?? DBNull.Value },
                new SqlParameter("ToEmail", SqlDbType.NVarChar) { Value = entity.ToEmail },
                new SqlParameter("ToDisplay", SqlDbType.NVarChar) { Value = (object)entity.ToDisplay ?? DBNull.Value },
                new SqlParameter("Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("Body", SqlDbType.NVarChar) { Value = (object)entity.Body ?? DBNull.Value },
                new SqlParameter("SendAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailQueue>("EXEC @ReturnValue = [svc].[usp_EmailQueue_Insert] "
                + "@FromEmail, @FromDisplay, @ToEmail, @ToDisplay, @Subject, @Body, @SendAtUtc", pvalues).Single();
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

            var pvalues = new[]
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

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("IsCancelled", SqlDbType.Bit) { Value = entity.IsCancelled },
                new SqlParameter("SendAtUtc", SqlDbType.DateTimeOffset) { Value = entity.SendAtUtc },
                new SqlParameter("DeliveredUtc", SqlDbType.DateTimeOffset) { Value = entity.DeliveredUtc.HasValue ? (object)entity.DeliveredUtc.Value : DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_EmailQueue>("EXEC @ReturnValue = [svc].[usp_EmailQueue_Update] "
                + "@Id, @IsCancelled, @SendAtUtc, @DeliveredUtc", pvalues).Single();
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
