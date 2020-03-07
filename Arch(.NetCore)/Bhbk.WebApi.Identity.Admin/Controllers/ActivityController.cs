using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    [Authorize(Policy = Constants.PolicyForUsers)]
    public class ActivityController : BaseController
    {
        private ActivityProvider _provider;

        public ActivityController(IConfiguration conf, IContextService instance)
        {
            _provider = new ActivityProvider(conf, instance);
        }

        [Route("v1/{activityValue}"), HttpGet]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult GetV1([FromRoute] string activityValue)
        {
            Guid activityID;
            tbl_Activities activity = null;

            if (Guid.TryParse(activityValue, out activityID))
                activity = UoW.Activities.Get(QueryExpressionFactory.GetQueryExpression<tbl_Activities>()
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
        [Authorize(Policy = Constants.PolicyForAdmins)]
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
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Activities>, IQueryable<tbl_Activities>>>>(
                                state.ToExpression<tbl_Activities>()))),

                    Total = UoW.Activities.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Activities>, IQueryable<tbl_Activities>>>>(
                            state.ToPredicateExpression<tbl_Activities>()))
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