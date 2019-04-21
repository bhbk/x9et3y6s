﻿using AutoMapper;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class ChangeControllerTests : IClassFixture<StartupTests>
    {
        private readonly IConfiguration _conf;
        private readonly IContextService _instance;
        private readonly IMapper _mapper;
        private readonly StartupTests _factory;

        public ChangeControllerTests(StartupTests factory)
        {
            _factory = factory;
            _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task Me_ChangeV1_Email_Fail()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            var newEmail = Base64.CreateString(4) + "-" + Constants.ApiTestUser;

            controller.SetUser(issuer.Id, user.Id);

            var model = new UserChangeEmail()
            {
                UserId = user.Id,
                CurrentEmail = Base64.CreateString(4),
                NewEmail = newEmail,
                NewEmailConfirm = newEmail
            };

            var result = await controller.ChangeEmailV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ChangeV1_Email_Success()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            var newEmail = Base64.CreateString(4) + "-" + Constants.ApiTestUser;

            controller.SetUser(issuer.Id, user.Id);

            var model = new UserChangeEmail()
            {
                UserId = user.Id,
                CurrentEmail = user.Email,
                NewEmail = newEmail,
                NewEmailConfirm = newEmail
            };

            var result = await controller.ChangeEmailV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [Fact]
        public async Task Me_ChangeV1_Password_Fail()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            var model = new UserChangePassword()
            {
                UserId = user.Id,
                CurrentPassword = Base64.CreateString(16),
                NewPassword = Constants.ApiTestUserPassNew,
                NewPasswordConfirm = Constants.ApiTestUserPassNew
            };

            var result = await controller.ChangePasswordV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ChangeV1_Password_Success()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            var model = new UserChangePassword()
            {
                UserId = user.Id,
                CurrentPassword = Constants.ApiTestUserPassCurrent,
                NewPassword = Constants.ApiTestUserPassNew,
                NewPasswordConfirm = Constants.ApiTestUserPassNew
            };

            var result = await controller.ChangePasswordV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [Fact]
        public async Task Me_ChangeV1_Phone_Fail()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            var newPhone = NumberAs.CreateString(10);

            controller.SetUser(issuer.Id, user.Id);

            var model = new UserChangePhone()
            {
                UserId = user.Id,
                CurrentPhoneNumber = newPhone,
                NewPhoneNumber = user.PhoneNumber,
                NewPhoneNumberConfirm = user.PhoneNumber
            };

            var result = await controller.ChangePhoneV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ChangeV1_Phone_Success()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            var newPhone = NumberAs.CreateString(10);

            controller.SetUser(issuer.Id, user.Id);

            var model = new UserChangePhone()
            {
                UserId = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = newPhone,
                NewPhoneNumberConfirm = newPhone
            };

            var result = await controller.ChangePhoneV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }
    }
}
