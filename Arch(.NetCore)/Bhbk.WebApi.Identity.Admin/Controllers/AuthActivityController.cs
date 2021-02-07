using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    public class AuthActivityController : BaseController
    {
        [Route("v1/{activityValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string activityValue)
        {
            Guid activityID;
            tbl_AuthActivity activity = null;

            if (Guid.TryParse(activityValue, out activityID))
                activity = uow.AuthActivity.Get(QueryExpressionFactory.GetQueryExpression<tbl_AuthActivity>()
                    .Where(x => x.Id == activityID).ToLambda())
                    .SingleOrDefault();

            if (activity == null)
            {
                ModelState.AddModelError(MessageType.ActivityNotFound.ToString(), $"activityID: { activityValue }");
                return NotFound(ModelState);
            }

            return Ok(map.Map<AuthActivityV1>(activity));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<AuthActivityV1>
                {
                    Data = map.Map<IEnumerable<AuthActivityV1>>(
                        uow.AuthActivity.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_AuthActivity>, IQueryable<tbl_AuthActivity>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_AuthActivity>().ApplyState(state)),
                                    new List<Expression<Func<tbl_AuthActivity, object>>>()
                                    {
                                        x => x.Audience,
                                        x => x.User,
                                    })),

                    Total = uow.AuthActivity.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_AuthActivity>, IQueryable<tbl_AuthActivity>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_AuthActivity>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }
    }
}