using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
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
        public async Task Admin_UserV1_AddToClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_AddToLogin_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var login = await _factory.UoW.LoginRepo.CreateAsync(
                new LoginCreate()
                {
                    Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1,
                    LoginKey = Strings.ApiUnitTestLogin1Key,
                    Enabled = true,
                    Immutable = false,
                });

            await _factory.UoW.CommitAsync();

            var response = await _endpoints.User_AddToLoginV1(access.token, user1.Id, login.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsInLoginAsync(user1.Id, login.Id);
            check.Should().BeTrue();
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_AddPasswordV1(access.token, Guid.NewGuid(), new UserAddPassword());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
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
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var remove = await _factory.UoW.UserRepo.RemovePasswordAsync(user1.Id);
            remove.Should().BeTrue();

            var response = await _endpoints.User_AddPasswordV1(access.token, user1.Id,
                new UserAddPassword()
                {
                    Id = user1.Id,
                    NewPassword = Strings.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Strings.ApiUnitTestUserPassNew,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsPasswordSetAsync(user1.Id);
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_CreateV1(access.token, new UserCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.User_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            user1.Immutable = true;

            await _factory.UoW.UserRepo.UpdateAsync(user1);

            response = await _endpoints.User_DeleteV1(access.token, user1.Id);

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
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_DeleteV1(access.token, user1.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = (await _factory.UoW.UserRepo.GetAsync(x => x.Id == user1.Id)).Any();
            check.Should().BeFalse();
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

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
            count.Should().Be(await _factory.UoW.UserRepo.CountAsync());
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var response = await _endpoints.User_GetClientsV1(access.token, user.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClientModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetClientsAsync(user.Id)).Count());
        }

        [Fact]
        public async Task Admin_UserV1_GetListLogins_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var response = await _endpoints.User_GetLoginsV1(access.token, user.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<LoginModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetLoginsAsync(user.Id)).Count());
        }

        [Fact]
        public async Task Admin_UserV1_GetListRoles_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var response = await _endpoints.User_GetRolesV1(access.token, user.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<RoleModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetRolesAsync(user.Id)).Count());
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_RemoveFromClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromLogin_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var login = await _factory.UoW.LoginRepo.CreateAsync(
                new LoginCreate()
                {
                    Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1,
                    LoginKey = Strings.ApiUnitTestLogin1Key,
                    Enabled = true,
                    Immutable = false,
                });

            await _factory.UoW.CommitAsync();

            var add = await _factory.UoW.UserRepo.AddToLoginAsync(user1, login);
            add.Should().BeTrue();

            await _factory.UoW.CommitAsync();

            var response = await _endpoints.User_RemoveFromLoginV1(access.token, user1.Id, login.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsInLoginAsync(user1.Id, login.Id);
            check.Should().BeFalse();
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_RemovePasswordV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
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
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_RemovePasswordV1(access.token, user1.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsPasswordSetAsync(user1.Id);
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_SetPasswordV1(access.token, Guid.NewGuid(), new UserAddPassword());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var model = new UserAddPassword()
            {
                Id = userA.Id,
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            var response = await _endpoints.User_SetPasswordV1(access.token, userA.Id, model);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.CheckPasswordAsync(userA.Id, model.NewPassword);
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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.User_UpdateV1(access.token, new UserModel());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.User_UpdateV1(access.token, new UserModel());

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

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            user1.FirstName += "(Updated)";

            var response = await _endpoints.User_UpdateV1(access.token, _factory.UoW.Transform.Map<UserModel>(user1));

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserModel>();

            check.FirstName.Should().Be(user1.FirstName);
        }
    }
}
