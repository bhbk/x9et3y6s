using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class IssuerControllerTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _sp;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly AdminClient _admin;

        public IssuerControllerTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _sp = fake.Server.Host.Services;
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
            _admin = new AdminClient(_conf, _uow.Situation, _client);
        }

        [Fact]
        public async Task Admin_IssuerV1_Create_Success()
        {
            var controller = new IssuerController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var model = new IssuerCreate()
            {
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestIssuer1,
                IssuerKey = RandomValues.CreateBase64String(32),
                Enabled = true,
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateIssuerV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [Fact]
        public async Task Admin_IssuerV1_Delete_Fail()
        {
            var controller = new IssuerController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            issuer.Immutable = true;
            await _uow.IssuerRepo.UpdateAsync(issuer);
            await _uow.CommitAsync();

            controller.SetUser(user.Id);

            var result = await controller.DeleteIssuerV1(issuer.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = (await _uow.IssuerRepo.GetAsync(x => x.Id == issuer.Id)).Any();
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Admin_IssuerV1_Delete_Success()
        {
            var controller = new IssuerController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteIssuerV1(issuer.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = (await _uow.IssuerRepo.GetAsync(x => x.Id == issuer.Id)).Any();
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_IssuerV1_GetList_Fail()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).CreateRandom(10);
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("name", "asc"));

            var pager = new TuplePager()
            {
                Filter = string.Empty,
                Orders = orders,
            };

            var response = await _admin.IssuerGetPagesV1(RandomValues.CreateBase64String(32), pager);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var access = await JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, new List<AppClient> { client }, user);

            response = await _admin.IssuerGetPagesV1(access.token, pager);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_IssuerV1_GetList_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).CreateRandom(10);
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, new List<AppClient> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("name", "asc"));

            var response = await _admin.IssuerGetPagesV1(access.token,
                new TuplePager()
                {
                    Filter = string.Empty,
                    Orders = orders,
                    Skip = 1,
                    Take = take,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var data = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<IssuerResult>>();
            var total = (int)ok["count"];

            data.Should().BeAssignableTo<IEnumerable<IssuerResult>>();
            data.Count().Should().Be(take);
            total.Should().Be(await _uow.IssuerRepo.Count());
        }

        [Fact]
        public async Task Admin_IssuerV1_GetListClients_Success()
        {
            var controller = new IssuerController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();

            var result = await controller.GetIssuerClientsV1(issuer.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<ClientResult>>().Subject;

            data.Count().Should().Be((await _uow.IssuerRepo.GetClientsAsync(issuer.Id)).Count());
        }

        [Fact]
        public async Task Admin_IssuerV1_Get_Success()
        {
            var controller = new IssuerController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();

            var result = await controller.GetIssuerV1(issuer.Id.ToString()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Id.Should().Be(issuer.Id);

            result = await controller.GetIssuerV1(issuer.Name) as OkObjectResult;
            ok = result.Should().BeOfType<OkObjectResult>().Subject;
            data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Id.Should().Be(issuer.Id);
        }

        [Fact]
        public async Task Admin_IssuerV1_Update_Success()
        {
            var controller = new IssuerController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new IssuerUpdate()
            {
                Id = issuer.Id,
                Name = Strings.ApiUnitTestIssuer1 + "(Updated)",
                IssuerKey = issuer.IssuerKey,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateIssuerV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
