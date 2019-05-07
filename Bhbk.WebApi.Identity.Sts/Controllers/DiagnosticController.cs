﻿using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        [Route("v1/status/{name}"), HttpGet]
        public IActionResult GetStatusV1([FromRoute] string name)
        {
            TaskType taskType;

            if (!Enum.TryParse<TaskType>(name, true, out taskType))
                return BadRequest();

            if (string.Equals(name, TaskType.MaintainRefreshes.ToString(), StringComparison.OrdinalIgnoreCase))
                return Ok(((MaintainRefreshesTask)Tasks.Single(x => x.GetType() == typeof(MaintainRefreshesTask))).Status);

            else if (string.Equals(name, TaskType.MaintainStates.ToString(), StringComparison.OrdinalIgnoreCase))
                return Ok(((MaintainStatesTask)Tasks.Single(x => x.GetType() == typeof(MaintainStatesTask))).Status);

            return BadRequest();
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersionV1()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
