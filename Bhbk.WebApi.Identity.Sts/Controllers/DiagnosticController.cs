using Bhbk.Lib.Identity.Interfaces;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IIdentityContext ioc)
            : base(ioc) { }

        [Route("v1/task"), HttpGet]
        [AllowAnonymous]
        public IActionResult GetTasks()
        {
            var sc = HttpContext.RequestServices.GetServices<IHostedService>();
            var task = (MaintainTokensTask)sc.Single(x => x.GetType() == typeof(MaintainTokensTask));

            return Ok(task.Status);
        }

        [Route("v1/version"), HttpGet]
        [AllowAnonymous]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
