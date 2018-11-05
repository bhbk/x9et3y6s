using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using System;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class ClaimControllerTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _sp;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;

        public ClaimControllerTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _sp = fake.Server.Host.Services;
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
        }

        [Fact]
        public async Task Admin_ClaimV1_Create_Success()
        {
            var controller = new ClaimController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var claim = new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue);

            controller.SetUser(user.Id);

            var result = await controller.CreateClaimV1(user.Id, claim) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<Claim>().Subject;

            data.Type.Should().Be(claim.Type);
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Success()
        {
            var controller = new ClaimController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var claim = new Claim(Strings.ApiUnitTestClaimType,
                Strings.ApiUnitTestClaimValue + "-" + RandomValues.CreateBase64String(4));

            var add = await _uow.CustomUserMgr.AddClaimAsync(user, claim);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            controller.SetUser(user.Id);

            var result = await controller.DeleteClaimV1(user.Id, claim) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = user.AppUserClaim.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Any();
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_ClaimV1_Get_Success()
        {
            var controller = new ClaimController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await controller.GetClaimsV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<Claim>>().Subject;

            data.Count().Should().Be(user.AppUserClaim.Count());
        }
    }
}
