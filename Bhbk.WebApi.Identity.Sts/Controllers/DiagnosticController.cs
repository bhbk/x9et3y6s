﻿using Bhbk.Lib.Identity.Interfaces;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("diagnostic")]
    [AllowAnonymous]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IIdentityContext ioc)
            : base(ioc) { }

        [Route("v1/task"), HttpGet]
        public IActionResult GetTaskStatus()
        {
            var task = (MaintainTokensTask)Tasks.Single(x => x.GetType() == typeof(MaintainTokensTask));

            return Ok(task.Status);
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
