using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    [Collection("MeTests")]
    public class ConfirmControllerTests
    {
        private readonly StartupTests _factory;

        public ConfirmControllerTests(StartupTests factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Me_ConfirmV1_Email_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.DestroyAsync();
                await _factory.TestData.CreateAsync();

                var controller = new ConfirmController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

                controller.SetUser(user.Id);

                var token = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                    .GenerateAsync(newEmail, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
                token.Should().NotBeNullOrEmpty();

                var result = await controller.ConfirmEmailV1(user.Id, newEmail,
                    RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
            }
        }

        [Fact]
        public async Task Me_ConfirmV1_Email_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.DestroyAsync();
                await _factory.TestData.CreateAsync();

                var controller = new ConfirmController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

                controller.SetUser(user.Id);

                var token = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                    .GenerateAsync(newEmail, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
                token.Should().NotBeNullOrEmpty();

                var result = await controller.ConfirmEmailV1(user.Id, newEmail, token) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));
            }
        }

        [Fact]
        public async Task Me_ConfirmV1_Password_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.DestroyAsync();
                await _factory.TestData.CreateAsync();

                var controller = new ConfirmController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var newPassword = RandomValues.CreateBase64String(12);

                controller.SetUser(user.Id);

                var token = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                    .GenerateAsync(newPassword, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
                token.Should().NotBeNullOrEmpty();

                var result = await controller.ConfirmPasswordV1(user.Id, newPassword,
                    RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
            }
        }

        [Fact]
        public async Task Me_ConfirmV1_Password_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.DestroyAsync();
                await _factory.TestData.CreateAsync();

                var controller = new ConfirmController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var newPassword = RandomValues.CreateBase64String(12);

                controller.SetUser(user.Id);

                var token = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                    .GenerateAsync(newPassword, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
                token.Should().NotBeNullOrEmpty();

                var result = await controller.ConfirmPasswordV1(user.Id, newPassword, token) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));
            }
        }

        [Fact]
        public async Task Me_ConfirmV1_PhoneNumber_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.DestroyAsync();
                await _factory.TestData.CreateAsync();

                var controller = new ConfirmController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var newPhoneNumber = RandomValues.CreateNumberAsString(10);

                controller.SetUser(user.Id);

                var token = await new TotpHelper(8, 10).GenerateAsync(newPhoneNumber, user);
                token.Should().NotBeNullOrEmpty();

                var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber,
                    RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
            }
        }

        [Fact]
        public async Task Me_ConfirmV1_PhoneNumber_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.DestroyAsync();
                await _factory.TestData.CreateAsync();

                var controller = new ConfirmController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var newPhoneNumber = RandomValues.CreateNumberAsString(10);

                controller.SetUser(user.Id);

                var token = await new TotpHelper(8, 10).GenerateAsync(newPhoneNumber, user);
                token.Should().NotBeNullOrEmpty();

                var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber, token) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));
            }
        }
    }
}
