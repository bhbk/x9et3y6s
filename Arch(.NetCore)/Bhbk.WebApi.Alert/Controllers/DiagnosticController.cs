using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Domain.Providers.Alert;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
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

        [Route("v1/version"), HttpGet]
        public IActionResult GetVersionV1()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
