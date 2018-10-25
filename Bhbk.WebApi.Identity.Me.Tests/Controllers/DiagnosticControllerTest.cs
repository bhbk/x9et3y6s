﻿using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

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
        public void Api_Me_DiagV1_GetStatus_Fail_Invalid()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetStatusV1(RandomValues.CreateAlphaNumericString(8)) as BadRequestResult;
            var ok = result.Should().BeOfType<BadRequestResult>().Subject;
        }

        [TestMethod]
        public void Api_Me_DiagV1_GetStatus_Success()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetStatusV1(Enums.TaskType.MaintainQuotes.ToString().ToLower()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public async Task Api_Me_DiagV1_GetSwagger_Success()
        {
            var result = await _owin.CreateClient().GetAsync("/help/index.html");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public void Api_Me_DiagV1_GetVersion_Success()
        {
            var controller = new DiagnosticController(_conf, _ioc, _tasks);

            var result = controller.GetVersionV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;

            data.Should().Be(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
