using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
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
    public class UserServiceTests
    {
        private readonly StartupTests _factory;

        public UserServiceTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_AddToClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_AddToLogin_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();
                var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var login = await _factory.UoW.LoginRepo.CreateAsync(
                    new LoginCreate()
                    {
                        Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin,
                        LoginKey = Strings.ApiUnitTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    });

                await _factory.UoW.CommitAsync();

                var result = await service.HttpClient.User_AddLoginV1(rop.token, testUser.Id, login.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await _factory.UoW.UserRepo.IsInLoginAsync(testUser.Id, login.Id);
                check.Should().BeTrue();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                (await _factory.UoW.UserRepo.RemovePasswordAsync(testUser.Id)).Should().BeTrue();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.HttpClient.User_AddPasswordV1(rop.token, testUser.Id,
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

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_CreateV1(rop.token, new UserCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_CreateV1(rop.token, new UserCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_CreateV1(rop.token, new UserCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var create = new UserCreate()
                {
                    Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser,
                    FirstName = "First-" + RandomValues.CreateBase64String(4),
                    LastName = "Last-" + RandomValues.CreateBase64String(4),
                    PhoneNumber = RandomValues.CreateNumberAsString(10),
                    LockoutEnabled = false,
                    HumanBeing = true,
                };

                var result = await service.HttpClient.User_CreateV1NoConfirm(rop.token, create);

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

                result = await service.HttpClient.User_CreateV1(rop.token, create);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<UserModel>().Should().BeAssignableTo<UserModel>();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                user1.Immutable = true;

                await _factory.UoW.UserRepo.UpdateAsync(user1);
                await _factory.UoW.CommitAsync();

                result = await service.HttpClient.User_DeleteV1(rop.token, user1.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_DeleteV1(rop.token, testUser.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = (await _factory.UoW.UserRepo.GetAsync(x => x.Id == testUser.Id)).Any();
                check.Should().BeFalse();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(_factory.UoW, issuer, user);

                var result = await service.HttpClient.User_DeleteRefreshesV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                rt = await JwtFactory.UserRefreshV2(_factory.UoW, issuer, user);

                result = await service.HttpClient.User_DeleteRefreshesV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                result = await service.HttpClient.User_DeleteRefreshV1(rop.token, user.Id, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(_factory.UoW, issuer, user);

                var result = await service.HttpClient.User_DeleteRefreshesV1(rop.token, user.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                rt = await JwtFactory.UserRefreshV2(_factory.UoW, issuer, user);

                var refresh = (await _factory.UoW.RefreshRepo.GetAsync(x => x.UserId == user.Id
                    && x.RefreshValue == rt)).Single();

                result = await service.HttpClient.User_DeleteRefreshV1(rop.token, user.Id, refresh.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();
                await _factory.TestData.CreateRandomAsync(3);

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("email", "asc"));

                var result = await service.HttpClient.User_GetV1(rop.token,
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

                result = await service.HttpClient.User_GetV1(rop.token, list.First().Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<UserModel>().Should().BeAssignableTo<UserModel>();

                result = await service.HttpClient.User_GetV1(rop.token, list.First().Email.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<UserModel>().Should().BeAssignableTo<UserModel>();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClaims_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.HttpClient.User_GetClaimsV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClaimModel>>();
                ok.Count().Should().Be((await _factory.UoW.UserRepo.GetClaimsAsync(user.Id)).Count());

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClients_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.HttpClient.User_GetClientsV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClientModel>>();
                ok.Count().Should().Be((await _factory.UoW.UserRepo.GetClientsAsync(user.Id)).Count());

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetLogins_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.HttpClient.User_GetLoginsV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<LoginModel>>();
                ok.Count().Should().Be((await _factory.UoW.UserRepo.GetLoginsAsync(user.Id)).Count());

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(_factory.UoW, issuer, user);

                var result = await service.HttpClient.User_GetRefreshesV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<IEnumerable<RefreshModel>>().Should().BeAssignableTo<IEnumerable<RefreshModel>>();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRoles_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.HttpClient.User_GetRolesV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<RoleModel>>();
                ok.Count().Should().Be((await _factory.UoW.UserRepo.GetRolesAsync(user.Id)).Count());

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_RemoveFromClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromLogin_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

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

                var result = await service.HttpClient.User_RemoveLoginV1(rop.token, testUser.Id, login.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await _factory.UoW.UserRepo.IsInLoginAsync(testUser.Id, login.Id);
                check.Should().BeFalse();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_RemovePasswordV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_RemovePasswordV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_RemovePasswordV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_RemovePasswordV1(rop.token, testUser.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await _factory.UoW.UserRepo.IsPasswordSetAsync(testUser.Id);
                check.Should().BeFalse();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var model = new UserAddPassword()
                {
                    UserId = testUser.Id,
                    NewPassword = Strings.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
                };

                var result = await service.HttpClient.User_SetPasswordV1(rop.token, testUser.Id, model);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await _factory.UoW.UserRepo.CheckPasswordAsync(testUser.Id, model.NewPassword);
                check.Should().BeTrue();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.User_UpdateV1(rop.token, new UserModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_UpdateV1(rop.token, new UserModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.User_UpdateV1(rop.token, new UserModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                testUser.FirstName += "(Updated)";

                var result = await service.HttpClient.User_UpdateV1(rop.token, _factory.UoW.Mapper.Map<UserModel>(testUser));
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                var check = ok.ToObject<UserModel>();
                check.FirstName.Should().Be(testUser.FirstName);

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}
