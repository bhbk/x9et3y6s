using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.DomainModels;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Internal.Models;
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

namespace Bhbk.WebApi.Identity.Admin.Tests.ControllerTests
{
    [Collection("AdminTests")]
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
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var login = await _factory.UoW.LoginRepo.CreateAsync(
                new LoginCreate()
                {
                    Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin,
                    LoginKey = Strings.ApiUnitTestLoginKey,
                    Enabled = true,
                    Immutable = false,
                });

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.User_AddToLoginV1(rop.token, testUser.Id, login.Id);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsInLoginAsync(testUser.Id, login.Id);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var remove = await _factory.UoW.UserRepo.RemovePasswordAsync(testUser.Id);
            remove.Should().BeTrue();

            var result = await _endpoints.User_AddPasswordV1(rop.token, testUser.Id,
                new UserAddPassword()
                {
                    UserId = testUser.Id,
                    NewPassword = Strings.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Strings.ApiUnitTestUserPassNew,
                });

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsPasswordSetAsync(testUser.Id);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_CreateV1(rop.token, new UserCreate());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_CreateV1(rop.token, new UserCreate());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_CreateV1(rop.token, new UserCreate());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Create_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var create = new UserCreate()
            {
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = true,
            };

            var result = await _endpoints.User_CreateV1NoConfirm(rop.token, create);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserModel>();

            create = new UserCreate()
            {
                IssuerId = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single().Id,
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = false,
            };

            result = await _endpoints.User_CreateV1(rop.token, create);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_DeleteV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_DeleteV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_DeleteV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
            user1.Immutable = true;

            await _factory.UoW.UserRepo.UpdateAsync(user1);
            await _factory.UoW.CommitAsync();

            result = await _endpoints.User_DeleteV1(rop.token, user1.Id);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_DeleteV1(rop.token, testUser.Id);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = (await _factory.UoW.UserRepo.GetAsync(x => x.Id == testUser.Id)).Any();
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_UserV1_Get_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_GetV1(rop.token, testUser.Id.ToString());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserModel>();

            result = await _endpoints.User_GetV1(rop.token, testUser.Email);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            check = ok.ToObject<UserModel>();
        }

        [Fact]
        public async Task Admin_UserV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();
            await _factory.TestData.CreateRandomAsync(3);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("email", "asc"));

            var result = await _endpoints.User_GetPageV1(rop.token,
                new CascadePager()
                {
                    Filter = string.Empty,
                    Orders = orders,
                    Skip = 1,
                    Take = take,
                });

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var result = await _endpoints.User_GetClientsV1(rop.token, user.Id.ToString());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClientModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetClientsAsync(user.Id)).Count());
        }

        [Fact]
        public async Task Admin_UserV1_GetListLogins_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var result = await _endpoints.User_GetLoginsV1(rop.token, user.Id.ToString());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<LoginModel>>();

            ok.Count().Should().Be((await _factory.UoW.UserRepo.GetLoginsAsync(user.Id)).Count());
        }

        [Fact]
        public async Task Admin_UserV1_GetListRoles_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var result = await _endpoints.User_GetRolesV1(rop.token, user.Id.ToString());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<RoleModel>>();

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
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var login = await _factory.UoW.LoginRepo.CreateAsync(
                new LoginCreate()
                {
                    Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin,
                    LoginKey = Strings.ApiUnitTestLoginKey,
                    Enabled = true,
                    Immutable = false,
                });

            await _factory.UoW.CommitAsync();

            var add = await _factory.UoW.UserRepo.AddToLoginAsync(testUser, login);
            add.Should().BeTrue();

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.User_RemoveFromLoginV1(rop.token, testUser.Id, login.Id);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsInLoginAsync(testUser.Id, login.Id);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_RemovePasswordV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_RemovePasswordV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_RemovePasswordV1(rop.token, Guid.NewGuid());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_RemovePasswordV1(rop.token, testUser.Id);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.IsPasswordSetAsync(testUser.Id);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var model = new UserAddPassword()
            {
                UserId = testUser.Id,
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            var result = await _endpoints.User_SetPasswordV1(rop.token, testUser.Id, model);

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserRepo.CheckPasswordAsync(testUser.Id, model.NewPassword);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            var result = await _endpoints.User_UpdateV1(rop.token, new UserModel());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_UpdateV1(rop.token, new UserModel());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
            result = await _endpoints.User_UpdateV1(rop.token, new UserModel());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Update_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = await JwtBuilder.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
            testUser.FirstName += "(Updated)";

            var result = await _endpoints.User_UpdateV1(rop.token, _factory.UoW.Shape.Map<UserModel>(testUser));

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserModel>();

            check.FirstName.Should().Be(testUser.FirstName);
        }
    }
}
