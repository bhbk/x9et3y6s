using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ControllerTests
{
    public class DiagnosticControllerTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;

        public DiagnosticControllerTests(StartupTests factory)
        {
            _factory = factory;
            _factory.CreateClient();
        }

        [Fact]
        public void Admin_DiagV1_GetStatus_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
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
        public void Admin_DiagV1_GetStatus_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var controller = new DiagnosticController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var result = controller.GetStatusV1(TaskType.MaintainActivity.ToString()) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<string>().Subject;

                result = controller.GetStatusV1(TaskType.MaintainUsers.ToString()) as OkObjectResult;
                ok = result.Should().BeOfType<OkObjectResult>().Subject;
                data = ok.Value.Should().BeAssignableTo<string>().Subject;
            }
        }

        [Fact]
        public void Admin_DiagV1_GetVersion_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
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
