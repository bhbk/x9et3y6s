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
            var ask = await _endpoints.DeviceCode_AskV1(
                new DeviceCodeRequestV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = Guid.NewGuid().ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Use_NotImplemented()
        {
            var dc = await _endpoints.DeviceCode_UseV1(
                new DeviceCodeV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    user_code = RandomValues.CreateBase64String(32),
                    device_code = RandomValues.CreateBase64String(32),
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = string.Empty,
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Empty,
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = Guid.NewGuid().ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = string.Empty,
                    grant_type = "device_code",
                });
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

            var ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
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

            var ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await ask.Content.ReadAsStringAsync());

            var dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user_code = (string)ok["user_code"],
                    device_code = (string)ok["device_code"],
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = string.Empty,
                    client = client.Id.ToString(),
                    user_code = (string)ok["user_code"],
                    device_code = (string)ok["device_code"],
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user_code = (string)ok["user_code"],
                    device_code = (string)ok["device_code"],
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Empty,
                    user_code = (string)ok["user_code"],
                    device_code = (string)ok["device_code"],
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user_code = RandomValues.CreateAlphaNumericString(32),
                    device_code = (string)ok["device_code"],
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user_code = (string)ok["user_code"],
                    device_code = RandomValues.CreateAlphaNumericString(32),
                    grant_type = "device_code",
                });
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

            var ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var rop_ok = JObject.Parse(await ask.Content.ReadAsStringAsync());

            var dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user_code = (string)rop_ok["user_code"],
                    device_code = (string)rop_ok["device_code"],
                    grant_type = "device_code",
                });
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
