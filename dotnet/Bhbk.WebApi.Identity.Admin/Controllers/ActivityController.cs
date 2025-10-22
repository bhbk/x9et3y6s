using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EF.Models;
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
    [Route("activities")]
    public class ActivityController : BaseController
    {
        [Route("v1/{activityValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string activityValue)
        {
            Guid activityID;
            tbl_UserAuthActivity activity = null;

            if (Guid.TryParse(activityValue, out activityID))
                activity = uow.UserAuthActivities.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserAuthActivity>()
                    .Where(x => x.Id == activityID).ToLambda(),
                        new List<Expression<Func<tbl_UserAuthActivity, object>>>()
                        {
                            x => x.tbl_AudienceAuthActivities,
                        })
                    .SingleOrDefault();

            if (activity == null)
            {
                ModelState.AddModelError(MessageType.ActivityNotFound.ToString(), $"activityID: { activityValue }");
                return NotFound(ModelState);
            }

            return Ok(map.Map<UserAuthActivityV1>(activity));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] PagerState state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PagerStateResult<UserAuthActivityV1>
                {
                    Data = map.Map<IEnumerable<UserAuthActivityV1>>(
                        uow.UserAuthActivities.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_UserAuthActivity>, IQueryable<tbl_UserAuthActivity>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_UserAuthActivity>().ApplyState(state)),
                                    new List<Expression<Func<tbl_UserAuthActivity, object>>>()
                                    {
                                        x => x.tbl_AudienceAuthActivities,
                                        x => x.User,
                                    })),

                    Total = uow.UserAuthActivities.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_UserAuthActivity>, IQueryable<tbl_UserAuthActivity>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_UserAuthActivity>().ApplyPredicate(state)))
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