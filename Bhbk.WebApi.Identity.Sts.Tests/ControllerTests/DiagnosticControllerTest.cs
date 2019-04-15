using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    [Collection("StsTests")]
    public class DiagnosticControllerTest
    {
        private readonly StartupTest _factory;

        public DiagnosticControllerTest(StartupTest factory)
        {
            _factory = factory;
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

                var result = controller.GetStatusV1(TaskType.MaintainRefreshes.ToString()) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;

                result = controller.GetStatusV1(TaskType.MaintainStates.ToString()) as OkObjectResult;
                ok = result.Should().BeOfType<OkObjectResult>().Subject;
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
