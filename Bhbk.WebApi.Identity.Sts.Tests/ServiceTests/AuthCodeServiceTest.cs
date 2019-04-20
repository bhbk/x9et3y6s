using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class AuthCodeServiceTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public AuthCodeServiceTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Instance, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Ask_NotImplemented()
        {
            var ask = await _endpoints.AuthCode_AskV1(
                new AuthCodeAskV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    response_type = "code",
                    scope = RandomValues.CreateBase64String(8),
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Use_NotImplemented()
        {
            var ac = await _endpoints.AuthCode_UseV1(
                new AuthCodeV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = RandomValues.CreateBase64String(8),
                    state = RandomValues.CreateBase64String(8),
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_Issuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeAskV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_User()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = Guid.NewGuid();

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
