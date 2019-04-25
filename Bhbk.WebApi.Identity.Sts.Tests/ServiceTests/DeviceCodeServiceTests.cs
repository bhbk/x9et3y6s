using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class DeviceCodeServiceTests
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _client;
        private readonly IStsService _service;

        public DeviceCodeServiceTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _service = new StsService(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Ask_NotImplemented()
        {
            var ask = await _service.Raw.DeviceCode_AskV1(
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

            var result = await _service.Raw.DeviceCode_ActionV1(RandomValues.CreateBase64String(8), Guid.NewGuid().ToString(), RandomValues.CreateAlphaNumericString(8));

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            result = await _service.Raw.DeviceCode_ActionV1(rop.token, Guid.NewGuid().ToString(), RandomValues.CreateAlphaNumericString(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Use_NotImplemented()
        {
            var dc = await _service.Raw.DeviceCode_UseV1(
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

            var ask = await _service.Raw.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _service.Raw.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = string.Empty,
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _service.Raw.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _service.Raw.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Empty,
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ask = await _service.Raw.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = Guid.NewGuid().ToString(),
                    grant_type = "device_code",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _service.Raw.DeviceCode_AskV2(
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

            var ask = _service.DeviceCode_AskV2(
                new DeviceCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    grant_type = "device_code",
                });

            ask.Should().BeAssignableTo<DeviceCodeV2>();
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Decide_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

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
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.DeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultNormalUserPassword,
                });

            var dc = await _service.Raw.DeviceCode_ActionV2(RandomValues.CreateBase64String(32), state.StateValue, ActionType.Allow.ToString());
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            dc = await _service.Raw.DeviceCode_ActionV2(rop.access_token, RandomValues.CreateBase64String(32), ActionType.Allow.ToString());
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            dc = await _service.Raw.DeviceCode_ActionV2(rop.access_token, state.StateValue, RandomValues.CreateAlphaNumericString(8));
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

            var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

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
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.DeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultNormalUserPassword,
                });

            var dc = await _service.Raw.DeviceCode_ActionV2(rop.access_token, state.StateValue, ActionType.Allow.ToString());
            dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            dc.StatusCode.Should().Be(HttpStatusCode.NoContent);

            dc = await _service.Raw.DeviceCode_ActionV2((string)rop.access_token, state.StateValue, ActionType.Deny.ToString());
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

            var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

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
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.DeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            var dc = await _service.Raw.DeviceCode_UseV2(
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

            dc = await _service.Raw.DeviceCode_UseV2(
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

            dc = await _service.Raw.DeviceCode_UseV2(
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

            dc = await _service.Raw.DeviceCode_UseV2(
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

            dc = await _service.Raw.DeviceCode_UseV2(
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

            dc = await _service.Raw.DeviceCode_UseV2(
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

            var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

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
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.DeviceCodeTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            //the state needs to be approved... would be done by user...
            state.StateDecision = true;

            await _factory.UoW.StateRepo.UpdateAsync(state);
            await _factory.UoW.CommitAsync();

            var dc = _service.DeviceCode_UseV2(
                new DeviceCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user_code = secret,
                    device_code = state.StateValue,
                    grant_type = "device_code",
                });
            dc.Should().BeAssignableTo<UserJwtV2>();

            JwtHelper.CanReadToken(dc.access_token).Should().BeTrue();

            var dc_claims = JwtHelper.ReadJwtToken(dc.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            dc_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            dc_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
