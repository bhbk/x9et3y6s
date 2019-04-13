using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        [Route("v1/status/{name}"), HttpGet]
        public IActionResult GetStatusV1([FromRoute] string name)
        {
            if (string.Equals(name, TaskType.QueueEmails.ToString(), StringComparison.OrdinalIgnoreCase))
                return Ok(((QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask))).Status);

            if (string.Equals(name, TaskType.QueueTexts.ToString(), StringComparison.OrdinalIgnoreCase))
                return Ok(((QueueTextTask)Tasks.Single(x => x.GetType() == typeof(QueueTextTask))).Status);

            return BadRequest();
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersionV1()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
