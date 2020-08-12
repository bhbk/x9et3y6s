using Bhbk.Lib.Common.Services;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    public class DiagnosticControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly IConfiguration conf;
        private readonly IContextService instance;
        private readonly BaseControllerTests _factory;

        public DiagnosticControllerTests(BaseControllerTests factory)
        {
            _factory = factory;
            _factory.CreateClient();

            conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
        }

        [Fact]
        public void Sts_DiagV1_GetVersion_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var controller = new DiagnosticController(conf, instance);
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
