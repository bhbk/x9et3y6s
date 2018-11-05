using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class DiagnosticControllerTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _sp;

        public DiagnosticControllerTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _sp = fake.Server.Host.Services;
        }

        [Fact]
        public async Task Me_DiagV1_CheckSwagger_Success()
        {
            var result = await _client.GetAsync($"help/index.html");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Me_DiagV1_GetStatus_Fail()
        {
            var controller = new DiagnosticController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            var result = controller.GetStatusV1(RandomValues.CreateAlphaNumericString(8)) as BadRequestResult;
            var ok = result.Should().BeOfType<BadRequestResult>().Subject;
        }

        [Fact]
        public void Me_DiagV1_GetStatus_Success()
        {
            var controller = new DiagnosticController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            var result = controller.GetStatusV1(Enums.TaskType.MaintainQuotes.ToString().ToLower()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [Fact]
        public void Me_DiagV1_GetVersion_Success()
        {
            var controller = new DiagnosticController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            var result = controller.GetVersionV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;

            data.Should().Be(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
        }
    }
}
