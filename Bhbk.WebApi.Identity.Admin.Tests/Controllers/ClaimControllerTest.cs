using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.DomainModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
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
    public class ClaimControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AdminClient _endpoints;

        public ClaimControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AdminClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Admin_ClaimV1_Create_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.Claim_CreateV1(access.token, new ClaimCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.Claim_CreateV1(access.token, new ClaimCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_ClaimV1_Create_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var create = new ClaimCreate()
            {
                IssuerId = issuer.Id,
                Type = Strings.ApiUnitTestClaim1,
                Value = RandomValues.CreateBase64String(8),
            };

            var response = await _endpoints.Claim_CreateV1(access.token, create);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<ClaimModel>();
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.Claim_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.Claim_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var model = (await _factory.UoW.ClaimRepo.GetAsync()).First();
            model.Immutable = true;

            await _factory.UoW.ClaimRepo.UpdateAsync(model);
            await _factory.UoW.CommitAsync();

            response = await _endpoints.Claim_DeleteV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var claim = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Immutable == false)).First();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.Claim_DeleteV1(access.token, claim.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Id == claim.Id)).Any();
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_ClaimV1_Get_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();
            var claim = (await _factory.UoW.ClaimRepo.GetAsync(x => x.IssuerId == issuer.Id && x.Immutable == false)).First();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.Claim_GetV1(access.token, claim.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<ClaimModel>();

            response = await _endpoints.Claim_GetV1(access.token, claim.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            check = ok.ToObject<ClaimModel>();
        }

        [Fact]
        public async Task Admin_ClaimV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();
            await _factory.TestData.CreateRandomAsync(3);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("type", "asc"));

            for (int i = 0; i <= take; i++)
                await _factory.UoW.ClaimRepo.CreateAsync(new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = "ClaimTest-" + RandomValues.CreateBase64String(32),
                    Value = RandomValues.CreateBase64String(8),
                    Immutable = false,
                });

            await _factory.UoW.CommitAsync();

            var response = await _endpoints.Claim_GetV1(access.token,
                new CascadePager()
                {
                    Filter = string.Empty,
                    Orders = orders,
                    Skip = 1,
                    Take = take,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClaimModel>>();
            var count = (int)ok["count"];

            list.Should().BeAssignableTo<IEnumerable<ClaimModel>>();
            list.Count().Should().Be(take);
            count.Should().Be(await _factory.UoW.ClaimRepo.CountAsync());
        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.Claim_UpdateV1(access.token, new ClaimModel());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.Claim_UpdateV1(access.token, new ClaimModel());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var claim = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Immutable == false)).First();
            claim.Value += "(Updated)";

            var response = await _endpoints.Claim_UpdateV1(access.token, _factory.UoW.Transform.Map<ClaimModel>(claim));

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<ClaimModel>();

            check.Value.Should().Be(claim.Value);
        }
    }
}
