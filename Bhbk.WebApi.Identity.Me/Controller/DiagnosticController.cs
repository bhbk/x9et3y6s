using Bhbk.Lib.Identity.Infrastructure;
using System.Reflection;
using System.Web.Http;

namespace Bhbk.WebApi.Identity.Me.Controller
{
    [RoutePrefix("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/version"), HttpGet]
        public IHttpActionResult GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
