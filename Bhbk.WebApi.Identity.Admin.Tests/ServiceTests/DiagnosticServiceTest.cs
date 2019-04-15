using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    [Collection("AdminTests")]
    public class DiagnosticServiceTest
    {
        private readonly StartupTest _factory;

        public DiagnosticServiceTest(StartupTest factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Admin_DiagV1_CheckSwagger_Success()
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
