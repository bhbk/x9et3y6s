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
    public class DeviceCodeControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public DeviceCodeControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Ask_NotImplemented()
        {
            var ask = await _endpoints.DeviceCode_AskV1(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Use_NotImplemented()
        {
            var ask = await _endpoints.DeviceCode_UseV1(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), RandomValues.CreateBase64String(32));
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var ask = await _endpoints.DeviceCode_AskV2(Guid.NewGuid().ToString(), client.Id.ToString(), user.Id.ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(string.Empty, client.Id.ToString(), user.Id.ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _endpoints.DeviceCode_AskV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(issuer.Id.ToString(), string.Empty, user.Id.ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _endpoints.DeviceCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), Guid.NewGuid().ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), string.Empty);
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var ask = await _endpoints.DeviceCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await ask.Content.ReadAsStringAsync());

            var check = ok.ToObject<AuthCodeV2>();
            check.Should().BeAssignableTo<AuthCodeV2>();
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Use_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var ask = await _endpoints.DeviceCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await ask.Content.ReadAsStringAsync());

            var dc = await _endpoints.DeviceCode_UseV2(Guid.NewGuid().ToString(), client.Id.ToString(), (string)ok["user_code"], (string)ok["device_code"]);
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _endpoints.DeviceCode_UseV2(string.Empty, client.Id.ToString(), (string)ok["user_code"], (string)ok["device_code"]);
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), (string)ok["user_code"], (string)ok["device_code"]);
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _endpoints.DeviceCode_UseV2(issuer.Id.ToString(), string.Empty, (string)ok["user_code"], (string)ok["device_code"]);
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), RandomValues.CreateAlphaNumericString(32), (string)ok["device_code"]);
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), (string)ok["user_code"], RandomValues.CreateAlphaNumericString(32));
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Use_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var ask = await _endpoints.DeviceCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString());
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var rop_ok = JObject.Parse(await ask.Content.ReadAsStringAsync());

            var dc = await _endpoints.DeviceCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), (string)rop_ok["user_code"], (string)rop_ok["device_code"]);
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.OK);

            var dc_ok = JObject.Parse(await dc.Content.ReadAsStringAsync());

            var dc_check = dc_ok.ToObject<UserJwtV2>();
            dc_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(dc_check.access_token).Should().BeTrue();

            var dc_claims = JwtBuilder.ReadJwtToken(dc_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            dc_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer1);
            dc_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
