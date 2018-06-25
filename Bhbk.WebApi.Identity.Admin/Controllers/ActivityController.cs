using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    public class ActivityController : BaseController
    {
        public ActivityController() { }

        public ActivityController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetActivity([FromQuery] UrlFilter filter = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (filter == null)
                filter = new UrlFilter(20, 1, "created", "descending");

            var activity = IoC.Activity.Get().AsQueryable()
                .OrderBy(filter.OrderBy + " " + filter.Sort)
                .Skip(Convert.ToInt32((filter.PageNum - 1) * filter.PageSize))
                .Take(Convert.ToInt32(filter.PageSize));

            var result = activity.Select(x => new ActivityFactory<AppActivity>(x).Evolve()).ToList();

            return Ok(result);
        }
    }
}