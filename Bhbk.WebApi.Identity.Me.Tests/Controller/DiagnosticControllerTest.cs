using Bhbk.WebApi.Identity.Me.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Bhbk.WebApi.Identity.Me.Tests.Controller
{
    [TestClass]
    public class DiagnosticControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public DiagnosticControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public void Api_Me_Diagnostic_GetVersion_Success()
        {
            var controller = new DiagnosticController(UoW);
            var result = controller.GetVersion() as OkNegotiatedContentResult<string>;

            result.Should().NotBeNull();
        }
    }
}
