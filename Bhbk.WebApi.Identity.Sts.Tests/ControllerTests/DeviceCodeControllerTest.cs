using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
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
                new DeviceCodeAskV1()
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
        public async Task Sts_OAuth2_DeviceCodeV1_Decide_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var result = await _endpoints.DeviceCode_DecideV1(RandomValues.CreateBase64String(8), Guid.NewGuid().ToString(), RandomValues.CreateAlphaNumericString(8));

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            result = await _endpoints.DeviceCode_DecideV1(rop.token, Guid.NewGuid().ToString(), RandomValues.CreateAlphaNumericString(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = string.Empty,
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Empty,
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = Guid.NewGuid().ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeAskV2()
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var ask = await _endpoints.DeviceCode_AskV2(
                new DeviceCodeAskV2()
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
        public async Task Sts_OAuth2_DeviceCodeV2_Decide_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var secret = await new TotpProvider(8, 10).GenerateAsync(user.SecurityStamp, user);

            var state = await _factory.UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.UnitTestsDeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultNormalUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var dc = await _endpoints.DeviceCode_DecideV2(RandomValues.CreateBase64String(32), state.StateValue, ActionType.Allow.ToString());
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            dc = await _endpoints.DeviceCode_DecideV2((string)ok["access_token"], RandomValues.CreateBase64String(32), ActionType.Allow.ToString());
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _endpoints.DeviceCode_DecideV2((string)ok["access_token"], state.StateValue, RandomValues.CreateAlphaNumericString(8));
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Decide_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var secret = await new TotpProvider(8, 10).GenerateAsync(user.SecurityStamp, user);

            var state = await _factory.UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.UnitTestsDeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultNormalUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var dc = await _endpoints.DeviceCode_DecideV2((string)ok["access_token"], state.StateValue, ActionType.Allow.ToString());
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NoContent);

            dc = await _endpoints.DeviceCode_DecideV2((string)ok["access_token"], state.StateValue, ActionType.Deny.ToString());
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Use_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var secret = await new TotpProvider(8, 10).GenerateAsync(user.SecurityStamp, user);

            var state = await _factory.UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.UnitTestsDeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            var dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user_code = secret,
                    device_code = state.StateValue,
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = string.Empty,
                    client = client.Id.ToString(),
                    user_code = secret,
                    device_code = state.StateValue,
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user_code = secret,
                    device_code = state.StateValue,
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Empty,
                    user_code = secret,
                    device_code = state.StateValue,
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
                    device_code = state.StateValue,
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user_code = secret,
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var secret = await new TotpProvider(8, 10).GenerateAsync(user.SecurityStamp, user);

            var state = await _factory.UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.UnitTestsDeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            //the state needs to be approved... would be done by user...
            state.StateDecision = true;

            await _factory.UoW.StateRepo.UpdateAsync(state);
            await _factory.UoW.CommitAsync();

            var dc = await _endpoints.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user_code = secret,
                    device_code = state.StateValue,
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
            dc_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            dc_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
