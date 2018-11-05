using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
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
    public class ActivityControllerTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly AdminClient _admin;

        public ActivityControllerTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
            _admin = new AdminClient(_conf, _uow.Situation, _client);
        }

        [Fact]
        public async Task Admin_ActivityV1_GetList_Fail()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).CreateRandom(10);
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("created", "desc"));

            var pager = new TuplePager()
            {
                Filter = string.Empty,
                Orders = orders,
            };

            var response = await _admin.ActivityGetPagesV1(RandomValues.CreateBase64String(32), pager);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var access = await JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, new List<AppClient> { client }, user);

            response = await _admin.ActivityGetPagesV1(access.token, pager);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_ActivityV1_GetList_Success()
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
            orders.Add(new Tuple<string, string>("created", "desc"));

            var pager = new TuplePager()
            {
                Filter = string.Empty,
                Orders = orders,
                Skip = 1,
                Take = take,
            };

            var response = await _admin.ActivityGetPagesV1(access.token, pager);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var data = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ActivityResult>>();
            var total = (int)ok["count"];

            data.Should().BeAssignableTo<IEnumerable<ActivityResult>>();
            data.Count().Should().Be(take);
            total.Should().Be(await _uow.ActivityRepo.Count());
        }
    }
}
