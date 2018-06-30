﻿using Bhbk.WebApi.Identity.Admin.Controllers;
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
        public void Api_Admin_Diag_GetStatus_MaintainActivity_Pass()
        {
            var controller = new DiagnosticController(TestIoC, TestTasks);

            var result = controller.GetStatus(BaseLib.TaskType.MaintainActivity.ToString()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Admin_Diag_GetStatus_MaintainNotify_Pass()
        {
            var controller = new DiagnosticController(TestIoC, TestTasks);

            var result = controller.GetStatus(BaseLib.TaskType.MaintainNotify.ToString().ToLower()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Admin_Diag_GetStatus_MaintainUsers_Pass()
        {
            var controller = new DiagnosticController(TestIoC, TestTasks);

            var result = controller.GetStatus(BaseLib.TaskType.MaintainUsers.ToString()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Admin_Diag_GetVersion_Pass()
        {
            var controller = new DiagnosticController(TestIoC, TestTasks);

            var result = controller.GetVersion() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;

            data.Should().Be(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
