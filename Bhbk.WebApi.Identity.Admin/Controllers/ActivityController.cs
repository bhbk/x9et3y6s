using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    public class ActivityController : BaseController
    {
        public ActivityController() { }

        public ActivityController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Authorize(Roles = "(Built-In) Administrators")]
        [Route("v1"), HttpGet]
        public IActionResult GetActivity()
        {
            var result = new List<ActivityResult>();
            var activities = IoC.Activity.Get();

            foreach (AppActivity entry in activities)
                result.Add(new ActivityFactory<AppActivity>(entry).Evolve());

            return Ok(result);
        }
    }
}