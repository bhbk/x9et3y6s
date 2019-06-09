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
using System.Linq.Dynamic.Core.Exceptions;
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
                activity = (await UoW.ActivityRepo.GetAsync(x => x.Id == activityID)).SingleOrDefault();

            if (activity == null)
            {
                ModelState.AddModelError(MessageType.ActivityNotFound.ToString(), $"activityID: { activityValue }");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ActivityModel>(activity));
        }

        [Route("v1/page"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> GetActivitiesV1([FromBody] DataPagerV3 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Activities, bool>> predicates;

            if (string.IsNullOrEmpty(model.Filter.First().Value))
                predicates = x => true;
            else
                predicates = x => x.ActivityType.Contains(model.Filter.First().Value, StringComparison.OrdinalIgnoreCase)
                || x.TableName.Contains(model.Filter.First().Value, StringComparison.OrdinalIgnoreCase)
                || x.OriginalValues.Contains(model.Filter.First().Value, StringComparison.OrdinalIgnoreCase)
                || x.CurrentValues.Contains(model.Filter.First().Value, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.ActivityRepo.CountAsync(predicates);
                var result = await UoW.ActivityRepo.GetAsync(predicates,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.Sort.First().Field, model.Sort.First().Dir)),
                    model.Skip,
                    model.Take);

                return Ok(new { 
                    Data = Mapper.Map<IEnumerable<ActivityModel>>(result), 
                    Total = total 
                });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }
    }
}