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
        // ── User activities ──────────────────────────────────────────────────────

        [Route("v1/users/{activityValue}"), HttpGet]
        public IActionResult GetUserActivityV1([FromRoute] string activityValue)
        {
            Guid activityID;
            tbl_UserActivity activity = null;

            if (Guid.TryParse(activityValue, out activityID))
                activity = uow.UserActivities.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserActivity>()
                    .Where(x => x.Id == activityID).ToLambda(),
                        new List<Expression<Func<tbl_UserActivity, object>>>()
                        {
                            x => x.tbl_AudienceActivities,
                        })
                    .SingleOrDefault();

            if (activity == null)
            {
                ModelState.AddModelError(MessageType.ActivityNotFound.ToString(), $"activityID: { activityValue }");
                return NotFound(ModelState);
            }

            return Ok(map.Map<UserActivityV1>(activity));
        }

        [Route("v1/users/page"), HttpPost]
        public IActionResult GetUserActivitiesV1([FromBody] PagerState state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PagerStateResult<UserActivityV1>
                {
                    Data = map.Map<IEnumerable<UserActivityV1>>(
                        uow.UserActivities.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_UserActivity>, IQueryable<tbl_UserActivity>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_UserActivity>().ApplyState(state)),
                                    new List<Expression<Func<tbl_UserActivity, object>>>()
                                    {
                                        x => x.tbl_AudienceActivities,
                                        x => x.User,
                                    })),

                    Total = uow.UserActivities.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_UserActivity>, IQueryable<tbl_UserActivity>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_UserActivity>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        // ── Audience activities ──────────────────────────────────────────────────

        [Route("v1/audiences/page"), HttpPost]
        public IActionResult GetAudienceActivitiesV1([FromBody] PagerState state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PagerStateResult<AudienceActivityV1>
                {
                    Data = map.Map<IEnumerable<AudienceActivityV1>>(
                        uow.AudienceActivities.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_AudienceActivity>, IQueryable<tbl_AudienceActivity>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_AudienceActivity>().ApplyState(state)),
                                    new List<Expression<Func<tbl_AudienceActivity, object>>>()
                                    {
                                        x => x.UserActivity,
                                        x => x.Audience,
                                    })),

                    Total = uow.AudienceActivities.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_AudienceActivity>, IQueryable<tbl_AudienceActivity>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_AudienceActivity>().ApplyPredicate(state)))
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
