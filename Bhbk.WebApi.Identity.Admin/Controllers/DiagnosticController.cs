using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        [Route("v1/status/{name}"), HttpGet]
        public IActionResult GetStatusV1([FromRoute] string name)
        {
            if (string.Equals(name, TaskType.MaintainActivity.ToString(), StringComparison.OrdinalIgnoreCase))
                return Ok(((MaintainActivityTask)Tasks.Single(x => x.GetType() == typeof(MaintainActivityTask))).Status);

            if (string.Equals(name, TaskType.MaintainUsers.ToString(), StringComparison.OrdinalIgnoreCase))
                return Ok(((MaintainUsersTask)Tasks.Single(x => x.GetType() == typeof(MaintainUsersTask))).Status);

            return BadRequest();
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersionV1()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
