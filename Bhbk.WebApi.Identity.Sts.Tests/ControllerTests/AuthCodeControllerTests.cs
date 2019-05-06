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
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    public class AuthCodeControllerTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public AuthCodeControllerTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Ask_NotImplemented()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            var ask = await service.Http.AuthCode_AskV1(
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
        public async Task Sts_OAuth2_AuthCodeV1_Auth_NotImplemented()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            var ac = await service.Http.AuthCode_AuthV1(
                new AuthCodeV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    grant_type = "authorization_code",
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = RandomValues.CreateBase64String(8),
                    state = RandomValues.CreateBase64String(8),
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_ClientNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var ask = await service.Http.AuthCode_AskV2(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_IssuerNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var ask = await service.Http.AuthCode_AskV2(
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
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_UserNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var ask = await service.Http.AuthCode_AskV2(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = Guid.NewGuid().ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_UrlNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var ask = await service.Http.AuthCode_AskV2(
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
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var ask = await controller.AuthCodeV2_Ask(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);

            HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(Constants.ApiUnitTestUriLink);
            HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_ClientNotExist()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])), user);
            var url = new Uri(Constants.ApiUnitTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])),
                }));

            uow.CommitAsync().Wait();

            var ac = await service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_IssuerNotExist()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])), user);
            var url = new Uri(Constants.ApiUnitTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])),
                }));

            uow.CommitAsync().Wait();

            var ac = await service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UrlNotExist()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])), user);
            var url = new Uri(Constants.ApiUnitTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])),
                }));

            uow.CommitAsync().Wait();

            var ac = await service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserNotExist()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])), user);
            var url = new Uri(Constants.ApiUnitTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])),
                }));

            uow.CommitAsync().Wait();

            var ac = await service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = Guid.NewGuid().ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserInvalidCode()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])), user);
            var url = new Uri(Constants.ApiUnitTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])),
                }));

            uow.CommitAsync().Wait();

            var ac = await service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = RandomValues.CreateBase64String(32),
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserInvalidState()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])), user);
            var url = new Uri(Constants.ApiUnitTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])),
                }));

            uow.CommitAsync().Wait();

            var ac = await service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = RandomValues.CreateBase64String(32),
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Success()
        {
            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var salt = conf["IdentityTenants:Salt"];
            salt.Should().Be(uow.IssuerRepo.Salt);

            var ask = uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])),
                })).Result;
            var ask_url = new Uri(Constants.ApiUnitTestUriLink);
            var ask_code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"])), user);

            uow.CommitAsync().Wait();

            var ac = service.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = ask_url.AbsoluteUri,
                    code = ask_code,
                    state = ask.StateValue,
                });
            ac.Should().BeAssignableTo<UserJwtV2>();

            JwtFactory.CanReadToken(ac.access_token).Should().BeTrue();

            var claims = JwtFactory.ReadJwtToken(ac.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
            claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
