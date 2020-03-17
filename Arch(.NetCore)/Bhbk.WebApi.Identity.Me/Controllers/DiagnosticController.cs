using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Domain.Providers.Me;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("diagnostic")]
    [Authorize(Roles = "Identity.Users, Identity.Admins")]
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

            if (string.Equals(name, TaskType.MaintainQuotes.ToString(), StringComparison.OrdinalIgnoreCase))
                return Ok(((MaintainQuotesTask)Tasks.Single(x => x.GetType() == typeof(MaintainQuotesTask))).Status);

            return BadRequest();
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersionV1()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
