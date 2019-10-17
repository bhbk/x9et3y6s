using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    public class ActivityController : BaseController
    {
        private ActivityProvider _provider;

        public ActivityController(IConfiguration conf, IContextService instance)
        {
            _provider = new ActivityProvider(conf, instance);
        }

        [Route("v1/{activityValue}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> GetActivityV1([FromRoute] string activityValue)
        {
            Guid activityID;
            tbl_Activities activity = null;

            if (Guid.TryParse(activityValue, out activityID))
                activity = (await UoW.Activities.GetAsync(new QueryExpression<tbl_Activities>()
                    .Where(x => x.Id == activityID)
                    .ToLambda())).SingleOrDefault();

            if (activity == null)
            {
                ModelState.AddModelError(MessageType.ActivityNotFound.ToString(), $"activityID: { activityValue }");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ActivityModel>(activity));
        }

        [Route("v1/page"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> GetActivitiesV1([FromBody] PageState model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateResult<ActivityModel>
                {
                    Data = Mapper.Map<IEnumerable<ActivityModel>>(
                        await UoW.Activities.GetAsync(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Activities>, IQueryable<tbl_Activities>>>>(
                                model.ToExpression<tbl_Activities>()))),

                    Total = await UoW.Activities.CountAsync(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Activities>, IQueryable<tbl_Activities>>>>(
                            model.ToPredicateExpression<tbl_Activities>()))
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