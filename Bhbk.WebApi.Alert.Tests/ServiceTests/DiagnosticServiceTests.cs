using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    [Collection("AlertTests")]
    public class DiagnosticServiceTests
    {
        private readonly StartupTests _factory;

        public DiagnosticServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_DiagV1_CheckSwagger_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var result = await owin.GetAsync($"help/index.html");
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
