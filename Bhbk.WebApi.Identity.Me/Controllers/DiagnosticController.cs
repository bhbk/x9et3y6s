using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IIdentityContext context)
            : base(context) { }

        [Route("v1/version"), HttpGet]
        [AllowAnonymous]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
