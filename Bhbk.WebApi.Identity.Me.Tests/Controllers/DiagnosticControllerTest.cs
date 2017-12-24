using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Bhbk.WebApi.Identity.Me.Tests.Controllers
{
    [TestClass]
    public class DiagnosticControllerTest : StartupTest
    {
        private TestServer _owin;

        public DiagnosticControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public void Api_Me_Diagnostic_GetTask_Success()
        {
            var controller = new DiagnosticController(IoC);

            var result = controller.GetTasks() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Me_Diagnostic_GetVersion_Success()
        {
            var controller = new DiagnosticController(IoC);

            var result = controller.GetVersion() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;

            data.Should().Be(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
