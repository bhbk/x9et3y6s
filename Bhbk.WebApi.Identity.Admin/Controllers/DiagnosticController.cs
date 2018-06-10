using Bhbk.Lib.Identity.Interfaces;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Admin.Controllers
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
            var task = (MaintainUsersTask)Tasks.Single(x => x.GetType() == typeof(MaintainUsersTask));

            return Ok(task.Status);
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
