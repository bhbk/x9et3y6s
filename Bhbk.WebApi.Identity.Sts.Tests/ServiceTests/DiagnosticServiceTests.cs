using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class DiagnosticServiceTests
    {
        private readonly StartupTests _factory;

        public DiagnosticServiceTests(StartupTests factory)
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
    }
}
