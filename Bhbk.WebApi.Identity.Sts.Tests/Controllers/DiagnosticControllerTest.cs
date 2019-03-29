using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTestCollection")]
    public class DiagnosticControllerTest
    {
        private readonly StartupTest _factory;

        public DiagnosticControllerTest(StartupTest factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Sts_DiagV1_CheckSwagger_Success()
        {
            using (var client = _factory.CreateClient())
            {
                var result = await client.GetAsync($"help/index.html");
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public void Sts_DiagV1_GetStatus_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new DiagnosticController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var result = controller.GetStatusV1(RandomValues.CreateAlphaNumericString(8)) as BadRequestResult;
                var ok = result.Should().BeOfType<BadRequestResult>().Subject;
            }
        }

        [Fact]
        public void Sts_DiagV1_GetStatus_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new DiagnosticController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var result = controller.GetStatusV1(TaskType.MaintainTokens.ToString()) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<string>().Subject;
            }
        }

        [Fact]
        public void Sts_DiagV1_GetVersion_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new DiagnosticController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var result = controller.GetVersionV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<string>().Subject;

                data.Should().Be(Assembly.GetAssembly(typeof(DiagnosticController)).GetName().Version.ToString());
            }
        }
    }
}
