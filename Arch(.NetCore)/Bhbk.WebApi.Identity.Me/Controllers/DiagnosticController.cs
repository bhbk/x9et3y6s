using Bhbk.Lib.Identity.Primitives.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("diagnostic")]
    [Authorize(Roles = DefaultConstants.RoleForViewers_Identity + ", " 
        + DefaultConstants.RoleForUsers_Identity + ", " 
        + DefaultConstants.RoleForAdmins_Identity)]
    public class DiagnosticController : BaseController
    {
        [Route("v1/version"), HttpGet]
        public IActionResult GetVersionV1()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
