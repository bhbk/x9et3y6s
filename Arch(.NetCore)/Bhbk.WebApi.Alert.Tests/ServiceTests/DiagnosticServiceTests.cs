using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    public class DiagnosticServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public DiagnosticServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public void Alert_DiagV1_CheckSwagger_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var result = owin.GetAsync($"help/index.html").Result;
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
