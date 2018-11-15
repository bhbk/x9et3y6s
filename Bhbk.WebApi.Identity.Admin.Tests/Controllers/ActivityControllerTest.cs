using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
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
    [Collection("AdminTestCollection")]
    public class ActivityControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AdminClient _endpoints;

        public ActivityControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AdminClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Admin_ActivityV1_GetList_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.Activity_GetV1(access.token, new TuplePager());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("created", "desc"));

            var model = new TuplePager()
            {
                Filter = string.Empty,
                Orders = orders,
            };

            response = await _endpoints.Activity_GetV1(access.token, model);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_ActivityV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();
            await _factory.TestData.CreateRandomAsync(3);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("created", "desc"));

            var model = new TuplePager()
            {
                Filter = string.Empty,
                Orders = orders,
                Skip = 1,
                Take = take,
            };

            var response = await _endpoints.Activity_GetV1(access.token, model);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ActivityResult>>();
            var count = (int)ok["count"];

            list.Should().BeAssignableTo<IEnumerable<ActivityResult>>();
            list.Count().Should().Be(take);

            count.Should().Be(await _factory.UoW.ActivityRepo.Count());
        }
    }
}
