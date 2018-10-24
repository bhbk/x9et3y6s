using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Factory;
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

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    public class ActivityController : BaseController
    {
        public ActivityController() { }

        public ActivityController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public IActionResult GetActivityV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var activity = IoC.Activity.Get()
                .OrderBy(model.OrderBy)
                .Skip(model.Skip)
                .Take(model.Take);

            var result = activity.Select(x => new ActivityFactory<AppActivity>(x).Evolve());

            return Ok(result);
        }
    }
}