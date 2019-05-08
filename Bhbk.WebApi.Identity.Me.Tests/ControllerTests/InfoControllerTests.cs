using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class InfoControllerTests : IClassFixture<StartupTests>
    {
        private readonly IConfiguration _conf;
        private readonly IContextService _instance;
        private readonly IMapper _mapper;
        private readonly StartupTests _factory;

        public InfoControllerTests(StartupTests factory)
        {
            _factory = factory;
            _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task Me_InfoV1_DeleteRefreshes_Fail()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            var result = await controller.DeleteUserRefreshV1(Guid.NewGuid()) as NotFoundObjectResult;
            result = await controller.DeleteUserRefreshesV1() as NotFoundObjectResult;
        }

        [Fact]
        public async Task Me_InfoV1_DeleteRefreshes_Success()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            for (int i = 0; i < 3; i++)
                await JwtFactory.UserRefreshV2(uow, _mapper, issuer, user);

            var refresh = (await uow.RefreshRepo.GetAsync(x => x.UserId == user.Id)).First();

            var result = await controller.DeleteUserRefreshV1(refresh.Id) as OkObjectResult;
            result = await controller.DeleteUserRefreshesV1() as OkObjectResult;
        }

        [Fact]
        public async Task Me_InfoV1_Get_Success()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            var result = await controller.GetUserV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task Me_InfoV1_GetRefreshes_Success()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            var result = await controller.GetUserRefreshesV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<RefreshModel>>().Subject;
        }

        [Fact]
        public async Task Me_InfoV1_SetPassword_Fail()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            var model = new UserChangePassword()
            {
                CurrentPassword = Constants.ApiTestUserPassCurrent,
                NewPassword = RandomValues.CreateBase64String(16),
                NewPasswordConfirm = RandomValues.CreateBase64String(16)
            };

            controller.SetUser(issuer.Id, user.Id);

            var result = await controller.SetUserPasswordV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await uow.UserRepo.CheckPasswordAsync(user.Id, model.NewPassword);
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Me_InfoV1_SetPassword_Success()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            var model = new UserChangePassword()
            {
                CurrentPassword = Constants.ApiTestUserPassCurrent,
                NewPassword = Constants.ApiTestUserPassNew,
                NewPasswordConfirm = Constants.ApiTestUserPassNew
            };

            controller.SetUser(issuer.Id, user.Id);

            var result = await controller.SetUserPasswordV1(model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await uow.UserRepo.CheckPasswordAsync(user.Id, model.NewPassword);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Me_InfoV1_SetTwoFactor_Success()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            var status = await uow.UserRepo.SetTwoFactorEnabledAsync(user.Id, false);
            status.Should().BeTrue();

            var result = await controller.SetTwoFactorV1(true) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [Fact]
        public async Task Me_InfoV1_Update_Success()
        {
            var controller = new InfoController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            controller.SetUser(issuer.Id, user.Id);

            var model = new UserModel()
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)",
                HumanBeing = false,
                Immutable = false,
            };

            var result = await controller.UpdateUserV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
