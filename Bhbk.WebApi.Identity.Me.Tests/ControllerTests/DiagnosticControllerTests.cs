using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class DiagnosticControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly IConfiguration _conf;
        private readonly IContextService _instance;
        private readonly BaseControllerTests _factory;

        public DiagnosticControllerTests(BaseControllerTests factory)
        {
            _factory = factory;
            _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
        }

        [Fact]
        public void Me_DiagV1_GetStatus_Fail()
        {
            var controller = new DiagnosticController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var result = controller.GetStatusV1(AlphaNumeric.CreateString(8)) as BadRequestResult;
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void Me_DiagV1_GetStatus_Success()
        {
            var controller = new DiagnosticController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var result = controller.GetStatusV1(TaskType.MaintainQuotes.ToString()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeAssignableTo<string>();
        }

        [Fact]
        public void Me_DiagV1_GetVersion_Success()
        {
            var controller = new DiagnosticController(_conf, _instance);
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
