using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
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
        public void Api_Admin_DiagV1_GetStatus_Success_MaintainActivity()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetStatusV1(BaseLib.TaskType.MaintainActivity.ToString()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Admin_DiagV1_GetStatus_Success_MaintainUsers()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetStatusV1(BaseLib.TaskType.MaintainUsers.ToString()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Admin_DiagV1_GetStatus_Success_QueueEmails()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetStatusV1(BaseLib.TaskType.QueueEmails.ToString().ToLower()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Admin_DiagV1_GetStatus_Success_QueueTexts()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetStatusV1(BaseLib.TaskType.QueueTexts.ToString().ToLower()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Admin_DiagV1_GetVersion_Success()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetVersionV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;

            data.Should().Be(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
