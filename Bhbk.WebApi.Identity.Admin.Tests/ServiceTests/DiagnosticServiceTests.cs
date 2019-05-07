﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class DiagnosticServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _http;

        public DiagnosticServiceTests(StartupTests factory)
        {
            _factory = factory;
            _http = _factory.CreateClient();
        }

        [Fact]
        public async Task Admin_DiagV1_CheckSwagger_Success()
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
