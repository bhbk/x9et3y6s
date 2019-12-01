using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("diagnostic")]
    [Authorize(Policy = RealConstants.PolicyForUsers)]
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
