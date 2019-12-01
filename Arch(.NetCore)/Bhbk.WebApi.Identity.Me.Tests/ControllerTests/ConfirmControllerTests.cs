using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class ConfirmControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public ConfirmControllerTests(BaseControllerTests factory) => _factory = factory;

        [Fact]
        public void Me_ConfirmV1_Email_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ConfirmController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newEmail = string.Format("{0}{1}", Base64.CreateString(4), user.Email);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire).Single();

                var token = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(newEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmEmailV1(user.Id, newEmail,
                    Base64.CreateString(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_ConfirmV1_Email_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ConfirmController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newEmail = string.Format("{0}{1}", Base64.CreateString(4), user.Email);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire).Single();

                var token = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(newEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmEmailV1(user.Id, newEmail, token) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        [Fact]
        public void Me_ConfirmV1_Password_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ConfirmController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newPassword = Base64.CreateString(12);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire).Single();

                var token = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(newPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPasswordV1(user.Id, newPassword,
                    Base64.CreateString(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_ConfirmV1_Password_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ConfirmController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newPassword = Base64.CreateString(12);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire).Single();

                var token = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(newPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPasswordV1(user.Id, newPassword, token) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        [Fact]
        public void Me_ConfirmV1_PhoneNumber_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ConfirmController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newPhoneNumber = NumberAs.CreateString(10);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var token = new TimeBasedTokenFactory(8, 10).Generate(newPhoneNumber, user);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPhoneV1(user.Id, newPhoneNumber,
                    Base64.CreateString(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_ConfirmV1_PhoneNumber_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ConfirmController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newPhoneNumber = NumberAs.CreateString(10);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var token = new TimeBasedTokenFactory(8, 10).Generate(newPhoneNumber, user);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPhoneV1(user.Id, newPhoneNumber, token) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }
    }
}
