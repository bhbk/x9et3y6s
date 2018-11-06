using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
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

            Expression<Func<ActivityModel, bool>> preds;
            Expression<Func<ActivityModel, object>> ords = x => string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2);

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.ActivityType.ToLower().Contains(model.Filter.ToLower())
                || x.TableName.ToLower().Contains(model.Filter.ToLower())
                || x.OriginalValues.ToLower().Contains(model.Filter.ToLower())
                || x.CurrentValues.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.ActivityRepo.Count(preds);
            var result = await UoW.ActivityRepo.GetAsync(preds, ords, null, model.Skip, model.Take);

            return Ok(new { Count = total, List = result });
        }
    }
}