using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IIdentityContext context)
            : base(context) { }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
