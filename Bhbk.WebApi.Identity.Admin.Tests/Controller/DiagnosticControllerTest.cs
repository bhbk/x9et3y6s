using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class DiagnosticControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public DiagnosticControllerTest()
        {
            //_owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Diagnostic_GetVersion_Success()
        {
            var controller = new DiagnosticController(UoW);
            var result = await controller.GetVersion() as OkNegotiatedContentResult<string>;

            result.Should().NotBeNull();
        }
    }
}
