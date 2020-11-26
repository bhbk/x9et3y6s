﻿using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_TBL;
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
    public class ActivityController : BaseController
    {
        [Route("v1/{activityValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string activityValue)
        {
            Guid activityID;
            tbl_Activity activity = null;

            if (Guid.TryParse(activityValue, out activityID))
                activity = UoW.Activities.Get(QueryExpressionFactory.GetQueryExpression<tbl_Activity>()
                    .Where(x => x.Id == activityID).ToLambda())
                    .SingleOrDefault();

            if (activity == null)
            {
                ModelState.AddModelError(MessageType.ActivityNotFound.ToString(), $"activityID: { activityValue }");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ActivityV1>(activity));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<ActivityV1>
                {
                    Data = Mapper.Map<IEnumerable<ActivityV1>>(
                        UoW.Activities.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Activity>, IQueryable<tbl_Activity>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Activity>().ApplyState(state)))),

                    Total = UoW.Activities.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Activity>, IQueryable<tbl_Activity>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Activity>().ApplyPredicate(state)))
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