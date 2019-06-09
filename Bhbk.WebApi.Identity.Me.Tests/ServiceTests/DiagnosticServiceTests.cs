using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ServiceTests
{
    public class DiagnosticServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;
        private readonly HttpClient _http;

        public DiagnosticServiceTests(BaseServiceTests factory)
        {
            _factory = factory;
            _http = _factory.CreateClient();
        }

        [Fact]
        public async Task Me_DiagV1_CheckSwagger_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var result = await _http.GetAsync($"help/index.html");
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
