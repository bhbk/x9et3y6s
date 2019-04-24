using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Repositories;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    [Collection("AdminTests")]
    public class LoginServiceTests
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _client;
        private readonly AdminRepository _endpoints;
        private readonly IAdminService _service;

        public LoginServiceTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AdminRepository(_factory.Conf, _factory.UoW.InstanceType, _client);
            _service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Admin_LoginV1_Create_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _service.Repo.Login_CreateV1(rop.token, new LoginCreate());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _service.Repo.Login_CreateV1(rop.token, new LoginCreate());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _service.Repo.Login_CreateV1(rop.token, new LoginCreate());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_LoginV1_Create_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var create = new LoginCreate()
            {
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin,
                LoginKey = Strings.ApiUnitTestLoginKey,
                Enabled = true,
                Immutable = false
            };

            var result = await _service.Repo.Login_CreateV1(rop.token, create);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            var check = ok.ToObject<LoginModel>();
        }

        [Fact]
        public async Task Admin_LoginV1_Delete_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _service.Repo.Login_DeleteV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _service.Repo.Login_DeleteV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _service.Repo.Login_DeleteV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var model = (await _factory.UoW.LoginRepo.GetAsync()).First();
            model.Immutable = true;

            await _factory.UoW.LoginRepo.UpdateAsync(model);
            await _factory.UoW.CommitAsync();

            result = await _service.Repo.Login_DeleteV1(rop.token, model.Id);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_LoginV1_Delete_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testLogin = (await _factory.UoW.LoginRepo.GetAsync(x => x.Name == Strings.ApiUnitTestLogin)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _service.Repo.Login_DeleteV1(rop.token, testLogin.Id);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = (await _factory.UoW.LoginRepo.GetAsync(x => x.Id == testLogin.Id)).Any();
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_LoginV1_Get_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var testLogin = (await _factory.UoW.LoginRepo.GetAsync(x => x.Name == Strings.ApiUnitTestLogin)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _service.Repo.Login_GetV1(rop.token, testLogin.Id.ToString());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            var check = ok.ToObject<LoginModel>();

            result = await _service.Repo.Login_GetV1(rop.token, testLogin.Name);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            check = ok.ToObject<LoginModel>();
        }

        [Fact]
        public async Task Admin_LoginV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();
            await _factory.TestData.CreateRandomAsync(3);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("name", "asc"));

            var response = await _service.Repo.Login_GetV1(rop.token,
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
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<LoginModel>>();
            var count = (int)ok["count"];

            list.Should().BeAssignableTo<IEnumerable<LoginModel>>();
            list.Count().Should().Be(take);
            count.Should().Be(await _factory.UoW.IssuerRepo.CountAsync());
        }

        [Fact]
        public async Task Admin_LoginV1_Update_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _service.Repo.Login_UpdateV1(rop.token, new LoginModel());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _service.Repo.Login_UpdateV1(rop.token, new LoginModel());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _service.Repo.Login_UpdateV1(rop.token, new LoginModel());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_LoginV1_Update_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var testLogin = (await _factory.UoW.LoginRepo.GetAsync(x => x.Name == Strings.ApiUnitTestLogin)).Single();
            testLogin.Name += "(Updated)";

            var result = await _service.Repo.Login_UpdateV1(rop.token, _factory.UoW.Mapper.Map<LoginModel>(testLogin));

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            var check = ok.ToObject<LoginModel>();

            check.Name.Should().Be(testLogin.Name);
        }
    }
}
