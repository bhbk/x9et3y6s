using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
            if (name.ToLower() == Enums.TaskType.QueueEmails.ToString().ToLower())
                return Ok(((QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask))).Status);

            if (name.ToLower() == Enums.TaskType.QueueTexts.ToString().ToLower())
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
