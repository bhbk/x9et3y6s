using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        public DeviceCodeServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Ask_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(conf, uow.InstanceType, owin);

                var ask = await service.Http.DeviceCode_AskV1(
                    new DeviceCodeAskV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        username = Guid.NewGuid().ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Use_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(conf, uow.InstanceType, owin);

                var dc = await service.Http.DeviceCode_UseV1(
                    new DeviceCodeV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        user_code = RandomValues.CreateBase64String(32),
                        device_code = RandomValues.CreateBase64String(32),
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

                ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = string.Empty,
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

                ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Empty,
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user = Guid.NewGuid().ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

                ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user = string.Empty,
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                var ask = service.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });

                ask.Should().BeAssignableTo<DeviceCodeV2>();
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Use_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

                var state = await uow.StateRepo.CreateAsync(
                    new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    });

                await uow.CommitAsync();

                var dc = await service.Http.DeviceCode_UseV2(
                    new DeviceCodeV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

                dc = await service.Http.DeviceCode_UseV2(
                    new DeviceCodeV2()
                    {
                        issuer = string.Empty,
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                dc = await service.Http.DeviceCode_UseV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

                dc = await service.Http.DeviceCode_UseV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Empty,
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                dc = await service.Http.DeviceCode_UseV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user_code = RandomValues.CreateAlphaNumericString(32),
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                dc = await service.Http.DeviceCode_UseV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = RandomValues.CreateAlphaNumericString(32),
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Use_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

                var state = await uow.StateRepo.CreateAsync(
                    new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    });

                await uow.CommitAsync();

                //the state needs to be approved... would be done by user...
                state.StateDecision = true;

                await uow.StateRepo.UpdateAsync(state);
                await uow.CommitAsync();

                var dc = service.DeviceCode_UseV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(dc.access_token).Should().BeTrue();

                var dc_claims = JwtFactory.ReadJwtToken(dc.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                dc_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                dc_claims.Value.Split(':')[1].Should().Be(salt);
            }
        }
    }
}
