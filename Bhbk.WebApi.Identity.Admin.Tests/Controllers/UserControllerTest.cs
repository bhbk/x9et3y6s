using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.EntityModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
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
    public class UserControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AdminClient _endpoints;

        public UserControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AdminClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_AddClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_AddPasswordV1(access.token, Guid.NewGuid(), new UserAddPassword());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.User_AddPasswordV1(access.token, Guid.NewGuid(), new UserAddPassword());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var model = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var remove = await _factory.UoW.UserRepo.RemovePasswordAsync(model);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var response = await _endpoints.User_AddPasswordV1(access.token, model.Id,
                new UserAddPassword()
                {
                    Id = model.Id,
                    NewPassword = Strings.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsPasswordSetAsync(model);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Admin_UserV1_Create_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_CreateV1(access.token, new UserCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.User_CreateV1(access.token, new UserCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Create_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var create = new UserCreate()
            {
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = true,
            };

            var response = await _endpoints.User_CreateV1NoConfirm(access.token, create);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserModel>();

            create = new UserCreate()
            {
                IssuerId = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single().Id,
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = false,
            };

            response = await _endpoints.User_CreateV1(access.token, create);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            check = ok.ToObject<UserModel>();
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.User_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var model = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            model.Immutable = true;

            await _factory.UoW.UserRepo.UpdateAsync(model);

            response = await _endpoints.User_DeleteV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var model = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_DeleteV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = (await _factory.UoW.UserRepo.GetAsync(x => x.Id == model.Id)).Any();
            check.Should().BeFalse();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_DeleteClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_Get_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            var model = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_GetV1(access.token, model.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserModel>();

            response = await _endpoints.User_GetV1(access.token, model.Email);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            check = ok.ToObject<UserModel>();
        }

        [Fact]
        public async Task Admin_UserV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();
            await _factory.TestData.CreateRandomAsync(3);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("email", "asc"));

            var response = await _endpoints.User_GetV1(access.token,
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
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<UserModel>>();
            var count = (int)ok["count"];

            list.Should().BeAssignableTo<IEnumerable<UserModel>>();
            list.Count().Should().Be(take);
            count.Should().Be(await _factory.UoW.UserRepo.Count());
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_GetListClaims_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_GetListClients_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var response = await _endpoints.User_GetClientsV1(access.token, user.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClientModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetClientsAsync(user)).Count());
        }

        [Fact]
        public async Task Admin_UserV1_GetListLogins_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var response = await _endpoints.User_GetLoginsV1(access.token, user.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<LoginModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetLoginsAsync(user)).Count());
        }

        [Fact]
        public async Task Admin_UserV1_GetListRoles_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var response = await _endpoints.User_GetRolesV1(access.token, user.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<RoleModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetRolesAsync_Deprecate(user)).Count());
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_RemovePasswordV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.User_RemovePasswordV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var model = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_RemovePasswordV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsPasswordSetAsync(model);
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_SetPasswordV1(access.token, Guid.NewGuid(), new UserAddPassword());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.User_SetPasswordV1(access.token, Guid.NewGuid(), new UserAddPassword());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var userA = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var model = new UserAddPassword()
            {
                Id = userA.Id,
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            var response = await _endpoints.User_SetPasswordV1(access.token, userA.Id, model);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.CheckPasswordAsync(userA, model.NewPassword);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Admin_UserV1_Update_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.User_UpdateV1(access.token, new UserUpdate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.User_UpdateV1(access.token, new UserUpdate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Update_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var model = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            model.FirstName += "(Updated)";

            var response = await _endpoints.User_UpdateV1(access.token, _factory.UoW.Convert.Map<UserUpdate>(model));

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserModel>();

            check.FirstName.Should().Be(model.FirstName);
        }
    }
}
