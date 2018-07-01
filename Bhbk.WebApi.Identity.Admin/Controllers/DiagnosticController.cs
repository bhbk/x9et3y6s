using Bhbk.Lib.Identity.Interfaces;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("diagnostic")]
    [AllowAnonymous]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1/status/{name}"), HttpGet]
        public IActionResult GetStatus([FromRoute] string name)
        {
            if (name.ToLower() == BaseLib.TaskType.MaintainActivity.ToString().ToLower())
                return Ok(((MaintainActivityTask)Tasks.Single(x => x.GetType() == typeof(MaintainActivityTask))).Status);

            if (name.ToLower() == BaseLib.TaskType.MaintainUsers.ToString().ToLower())
                return Ok(((MaintainUsersTask)Tasks.Single(x => x.GetType() == typeof(MaintainUsersTask))).Status);

            if (name.ToLower() == BaseLib.TaskType.QueueEmails.ToString().ToLower())
                return Ok(((QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask))).Status);

            if (name.ToLower() == BaseLib.TaskType.QueueTexts.ToString().ToLower())
                return Ok(((QueueTextTask)Tasks.Single(x => x.GetType() == typeof(QueueTextTask))).Status);

            return BadRequest();
        }

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
