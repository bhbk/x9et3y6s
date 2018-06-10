using Bhbk.Lib.Identity.Interfaces;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("diagnostic")]
    [AllowAnonymous]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1/task"), HttpGet]
        public IActionResult GetTaskStatus()
        {
            var task = (MaintainQuotesTask)Tasks.Single(x => x.GetType() == typeof(MaintainQuotesTask));

            return Ok(task.Status);
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
