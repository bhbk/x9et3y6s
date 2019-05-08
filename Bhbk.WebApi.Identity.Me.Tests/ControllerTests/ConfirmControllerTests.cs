﻿using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
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
using System.Threading.Tasks;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class ConfirmControllerTests : IClassFixture<StartupTests>
    {
        private readonly IConfiguration _conf;
        private readonly IContextService _instance;
        private readonly IMapper _mapper;
        private readonly StartupTests _factory;

        public ConfirmControllerTests(StartupTests factory)
        {
            _factory = factory;
            _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task Me_ConfirmV1_Email_Fail()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();

            var controller = new ConfirmController(conf, instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
            var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

            controller.SetUser(issuer.Id, user.Id);

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire)).Single();

            var token = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(newEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_Email_Success()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();

            var controller = new ConfirmController(conf, instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
            var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

            controller.SetUser(issuer.Id, user.Id);

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire)).Single();

            var token = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(newEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_Password_Fail()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();

            var controller = new ConfirmController(conf, instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
            var newPassword = RandomValues.CreateBase64String(12);

            controller.SetUser(issuer.Id, user.Id);

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire)).Single();

            var token = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(newPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_Password_Success()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();

            var controller = new ConfirmController(conf, instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
            var newPassword = RandomValues.CreateBase64String(12);

            controller.SetUser(issuer.Id, user.Id);

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire)).Single();

            var token = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(newPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_PhoneNumber_Fail()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();

            var controller = new ConfirmController(conf, instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
            var newPhoneNumber = RandomValues.CreateNumberAsString(10);

            controller.SetUser(issuer.Id, user.Id);

            var token = await new TotpHelper(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ConfirmV1_PhoneNumber_Success()
        {
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            var instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();

            var controller = new ConfirmController(conf, instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
            var newPhoneNumber = RandomValues.CreateNumberAsString(10);

            controller.SetUser(issuer.Id, user.Id);

            var token = await new TotpHelper(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }
    }
}
