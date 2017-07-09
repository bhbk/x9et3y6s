using Bhbk.WebApi.Identity.Sts.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http.Results;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controller
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
        public void Api_Sts_Diagnostic_GetVersion_Success()
        {
            var controller = new DiagnosticController(UoW);

            var result = controller.GetVersion() as OkNegotiatedContentResult<string>;
            result.Content.Should().BeAssignableTo(typeof(string));
        }
    }
}
