﻿using Bhbk.Lib.Identity.Infrastructure;
using System.Reflection;
using System.Web.Http;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("diagnostic")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController() { }

        public DiagnosticController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/version"), HttpGet]
        public async Task<IHttpActionResult> GetVersion()
        {
            return Ok(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}