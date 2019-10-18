using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Alert;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        private DiagnosticProvider _provider;

        public DiagnosticController(IConfiguration conf, IContextService instance)
        {
            _provider = new DiagnosticProvider(conf, instance);
        }

        [Route("v1/status/{name}"), HttpGet]
        public IActionResult GetStatusV1([FromRoute] string name)
        {
            TaskType taskType;

            if (!Enum.TryParse<TaskType>(name, true, out taskType))
                return BadRequest();

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
