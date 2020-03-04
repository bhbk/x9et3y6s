﻿using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class UrlRepository : GenericRepository<uvw_Urls>
    {
        public UrlRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Urls Create(uvw_Urls entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Urls> Create(IEnumerable<uvw_Urls> entities)
        {
            var results = new List<uvw_Urls>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Urls Delete(uvw_Urls entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Database.SqlQuery<uvw_Urls>("[svc].[usp_Url_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Urls> Delete(IEnumerable<uvw_Urls> entities)
        {
            var results = new List<uvw_Urls>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Urls> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Urls Update(uvw_Urls entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Urls> Update(IEnumerable<uvw_Urls> entities)
        {
            var results = new List<uvw_Urls>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
