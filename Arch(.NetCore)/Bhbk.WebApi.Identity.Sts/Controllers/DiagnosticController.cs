using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        [Route("v1/version"), HttpGet]
        public IActionResult GetVersionV1()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
