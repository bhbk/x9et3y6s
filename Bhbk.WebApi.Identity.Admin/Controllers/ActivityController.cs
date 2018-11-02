using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        public ActivityController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetActivityV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<AppActivity, bool>> expr;

            if (string.IsNullOrEmpty(model.Filter))
                expr = x => true;
            else
                expr = x => x.ActivityType.ToLower().Contains(model.Filter.ToLower())
                || x.TableName.ToLower().Contains(model.Filter.ToLower())
                || x.OriginalValues.ToLower().Contains(model.Filter.ToLower())
                || x.CurrentValues.ToLower().Contains(model.Filter.ToLower())
                || x.Created.ToString().ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.ActivityRepo.Count(expr);
            var activity = await UoW.ActivityRepo.GetAsync(expr,
                x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)).Skip(model.Skip).Take(model.Take));

            var result = activity.Select(x => UoW.Convert.Map<AppActivity>(x));

            return Ok(new { Count = total, Items = result });
        }
    }
}