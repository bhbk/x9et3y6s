using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
        public ActivityController() { }

        [Route("v1/pages"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> GetActivityPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<AppActivity, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.ActivityType.ToLower().Contains(model.Filter.ToLower())
                || x.TableName.ToLower().Contains(model.Filter.ToLower())
                || x.OriginalValues.ToLower().Contains(model.Filter.ToLower())
                || x.CurrentValues.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.ActivityRepo.Count(preds);

            var result = await UoW.ActivityRepo.GetAsync(preds, null, 
                x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)), model.Skip, model.Take);

            //Func<IQueryable<AppActivity>, IOrderedQueryable<AppActivity>> ords = x => GenerateOrders(model.Orders);
            //var result = await UoW.ActivityRepo.GetAsync(preds, null, ords, model.Skip, model.Take);

            return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<ActivityModel>>(result) });
        }

        public IOrderedQueryable<AppActivity> GenerateOrders(List<Tuple<string, string>> sortExpressions)
        {
            if ((sortExpressions == null) || (sortExpressions.Count <= 0))
                return null;

            IEnumerable<AppActivity> query = Enumerable.Empty<AppActivity>();
            IOrderedEnumerable<AppActivity> orderedQuery = Enumerable.Empty<AppActivity>().OrderBy(x => x);

            for (int i = 1; i < sortExpressions.Count; i++)
            {
                Func<AppActivity, object> expression = x => x.GetType()
                    .GetProperty(sortExpressions[i].Item1)
                    .GetValue(x);

                if (sortExpressions[i].Item2 == "asc")
                {
                    if(i == 1)
                        orderedQuery.OrderBy(expression);
                    else
                        orderedQuery.ThenBy(expression);
                }
                else
                {
                    if (i == 1)
                        orderedQuery.OrderByDescending(expression);
                    else
                        orderedQuery.ThenByDescending(expression);
                }
            }

            return orderedQuery.AsQueryable().OrderBy(x => x);
        }

        //public IEnumerable<T> GenerateOrders<T>(List<Tuple<string, string>> sortExpressions)
        //{
        //    // No sorting needed  
        //    if ((sortExpressions == null) || (sortExpressions.Count <= 0))
        //        return null;

        //    // Let us sort it  
        //    IEnumerable<T> query = null;
        //    IOrderedEnumerable<T> orderedQuery = null;

        //    for (int i = 0; i < sortExpressions.Count; i++)
        //    {
        //        // We need to keep the loop index, not sure why it is altered by the Linq.  
        //        var index = i;

        //        Func<T, object> expression = item => item.GetType()
        //         .GetProperty(sortExpressions[index].Item1)
        //         .GetValue(item, null);

        //        if (sortExpressions[index].Item2 == "asc")
        //        {
        //            orderedQuery = (index == 0) ? query.OrderBy(expression) :
        //                orderedQuery.ThenBy(expression);
        //        }
        //        else
        //        {
        //            orderedQuery = (index == 0) ? query.OrderByDescending(expression) :
        //                orderedQuery.ThenByDescending(expression);
        //        }
        //    }

        //    query = orderedQuery;

        //    return query;
        //}
    }
}