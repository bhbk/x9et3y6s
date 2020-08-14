using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class UrlRepository : GenericRepository<uvw_Url>
    {
        public UrlRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Url Create(uvw_Url entity)
        {
            throw new NotImplementedException();
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Database.SqlQuery<uvw_Url>("[svc].[usp_Url_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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
            throw new NotImplementedException();
        }

        public override uvw_Url Update(uvw_Url entity)
        {
            throw new NotImplementedException();
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
