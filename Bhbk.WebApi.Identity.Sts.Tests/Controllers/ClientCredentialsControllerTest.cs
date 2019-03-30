using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTestCollection")]
    public class ClientCredentialsControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public ClientCredentialsControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV1_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _endpoints.ClientCredentials_UseV1(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _endpoints.ClientCredentials_UseV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _endpoints.ClientCredentials_UseV2(issuer.Id.ToString(), client.Id.ToString(), RandomValues.CreateBase64String(16));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            client.Enabled = false;
            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            result = await _endpoints.ClientCredentials_UseV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Fail_Issuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _endpoints.ClientCredentials_UseV2(Guid.NewGuid().ToString(), client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            issuer.Enabled = false;
            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            result = await _endpoints.ClientCredentials_UseV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _endpoints.ClientCredentials_UseV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            var check = ok.ToObject<JwtV2>();
            check.Should().BeAssignableTo<JwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }
    }
}
