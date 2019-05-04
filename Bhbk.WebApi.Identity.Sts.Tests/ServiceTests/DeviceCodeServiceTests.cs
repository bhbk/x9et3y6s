using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
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
    public class DeviceCodeServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public DeviceCodeServiceTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Ask_NotImplemented()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

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
        public async Task Sts_OAuth2_DeviceCodeV1_Auth_NotImplemented()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                var dc = await service.Http.DeviceCode_AuthV1(
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
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail_IssuerNotExist()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

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
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail_ClientNotExist()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail_UserNotExist()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();

                var ask = await service.Http.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "device_code",
                        user = Guid.NewGuid().ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

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
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_ClientNotExist()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);
                var state = await uow.StateRepo.CreateAsync(
                    uow.Mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    }));

                uow.CommitAsync().Wait();

                var dc = await service.Http.DeviceCode_AuthV2(
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
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_DeviceCodeNotExist()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);
                var state = await uow.StateRepo.CreateAsync(
                    uow.Mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    }));

                uow.CommitAsync().Wait();

                var dc = await service.Http.DeviceCode_AuthV2(
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
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_IssuerNotExist()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);
                var state = await uow.StateRepo.CreateAsync(
                    uow.Mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    }));

                uow.CommitAsync().Wait();

                var dc = await service.Http.DeviceCode_AuthV2(
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
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_UserCodeNotExist()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);
                var state = await uow.StateRepo.CreateAsync(
                    uow.Mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    }));

                uow.CommitAsync().Wait();

                var dc = await service.Http.DeviceCode_AuthV2(
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
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);
                var state = await uow.StateRepo.CreateAsync(
                    uow.Mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    }));

                uow.CommitAsync().Wait();

                /*
                 * the state needs to be approved... would be done by user...
                 */
                state.StateDecision = true;

                uow.StateRepo.UpdateAsync(state).Wait();
                uow.CommitAsync().Wait();

                var dc = service.DeviceCode_AuthV2(
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
