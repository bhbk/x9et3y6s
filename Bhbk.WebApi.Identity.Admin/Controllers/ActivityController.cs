using Bhbk.Lib.Core.DomainModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    public class ActivityController : BaseController
    {
        public ActivityController() { }

        [Route("v1/page"), HttpGet]
        public async Task<IActionResult> GetActivityV1([FromQuery] SimplePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<TActivities, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.ActivityType.ToLower().Contains(model.Filter.ToLower())
                || x.TableName.ToLower().Contains(model.Filter.ToLower())
                || x.OriginalValues.ToLower().Contains(model.Filter.ToLower())
                || x.CurrentValues.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.ActivityRepo.CountAsync(preds);
                var result = await UoW.ActivityRepo.GetAsync(preds,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<ActivityModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetActivityV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<TActivities, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.ActivityType.ToLower().Contains(model.Filter.ToLower())
                || x.TableName.ToLower().Contains(model.Filter.ToLower())
                || x.OriginalValues.ToLower().Contains(model.Filter.ToLower())
                || x.CurrentValues.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.ActivityRepo.CountAsync(preds);
                var result = await UoW.ActivityRepo.GetAsync(preds,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<ActivityModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }
    }
}