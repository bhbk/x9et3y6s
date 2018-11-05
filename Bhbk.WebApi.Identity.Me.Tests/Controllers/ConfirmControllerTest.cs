using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class ConfirmControllerTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _sp;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;

        public ConfirmControllerTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _sp = fake.Server.Host.Services;
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
        }

        [Fact]
        public async Task Me_ConfirmV1_Email_Fail()
        {
            var controller = new ConfirmController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newEmail, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_Email_Success()
        {
            var controller = new ConfirmController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = this._sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newEmail, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_Password_Fail()
        {
            var controller = new ConfirmController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = this._sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPassword = RandomValues.CreateBase64String(12);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newPassword, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_Password_Success()
        {
            var controller = new ConfirmController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = this._sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPassword = RandomValues.CreateBase64String(12);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newPassword, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_PhoneNumber_Fail()
        {
            var controller = new ConfirmController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = this._sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPhoneNumber = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var token = await new TotpProvider(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_PhoneNumber_Success()
        {
            var controller = new ConfirmController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = this._sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPhoneNumber = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var token = await new TotpProvider(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }
    }
}
