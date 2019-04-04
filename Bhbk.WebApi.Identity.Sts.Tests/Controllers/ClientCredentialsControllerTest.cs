using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTests")]
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
            var cc = await _endpoints.ClientCredential_UseV1(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), RandomValues.CreateAlphaNumericString(8));
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var cc = await _endpoints.ClientCredential_UseV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            cc = await _endpoints.ClientCredential_UseV2(issuer.Id.ToString(), client.Id.ToString(), RandomValues.CreateBase64String(16));
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            cc = await _endpoints.ClientCredential_UseV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Fail_Issuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var cc = await _endpoints.ClientCredential_UseV2(Guid.NewGuid().ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            cc = await _endpoints.ClientCredential_UseV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var cc = await _endpoints.ClientCredential_UseV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await cc.Content.ReadAsStringAsync());
            var check = ok.ToObject<ClientJwtV2>();
            check.Should().BeAssignableTo<ClientJwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();

            var claims = JwtBuilder.ReadJwtToken(check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer1);
            claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
