using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
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

            Expression<Func<tbl_Activities, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.ActivityType.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.TableName.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.OriginalValues.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.CurrentValues.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.ActivityRepo.CountAsync(preds);
                var result = await UoW.ActivityRepo.GetAsync(preds,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Mapper.Map<IEnumerable<ActivityModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

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

            Expression<Func<tbl_Activities, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.ActivityType.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.TableName.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.OriginalValues.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.CurrentValues.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.ActivityRepo.CountAsync(preds);
                var result = await UoW.ActivityRepo.GetAsync(preds,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Mapper.Map<IEnumerable<ActivityModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }
    }
}