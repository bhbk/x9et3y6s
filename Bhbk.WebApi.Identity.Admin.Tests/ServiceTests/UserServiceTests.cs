using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var login = await uow.LoginRepo.CreateAsync(
                    new LoginCreate()
                    {
                        Name = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestLogin,
                        LoginKey = Constants.ApiUnitTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    });

                await uow.CommitAsync();

                var result = await service.Http.User_AddLoginV1(rop.token, testUser.Id, login.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await uow.UserRepo.IsInLoginAsync(testUser.Id, login.Id);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_AddPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                (await uow.UserRepo.RemovePasswordAsync(testUser.Id)).Should().BeTrue();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_AddPasswordV1(rop.token, testUser.Id,
                    new UserAddPassword()
                    {
                        UserId = testUser.Id,
                        NewPassword = Constants.ApiUnitTestUserPassNew,
                        NewPasswordConfirm = Constants.ApiUnitTestUserPassNew,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await uow.UserRepo.IsPasswordSetAsync(testUser.Id);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_CreateV1(rop.token, new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_CreateV1(rop.token, new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_CreateV1(rop.token, new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var create = new UserCreate()
                {
                    Email = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestUser,
                    FirstName = "First-" + RandomValues.CreateBase64String(4),
                    LastName = "Last-" + RandomValues.CreateBase64String(4),
                    PhoneNumber = RandomValues.CreateNumberAsString(10),
                    LockoutEnabled = false,
                    HumanBeing = true,
                };

                var result = await service.Http.User_CreateV1NoConfirm(rop.token, create);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                var check = ok.ToObject<UserModel>();

                create = new UserCreate()
                {
                    IssuerId = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single().Id,
                    Email = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestUser,
                    FirstName = "First-" + RandomValues.CreateBase64String(4),
                    LastName = "Last-" + RandomValues.CreateBase64String(4),
                    PhoneNumber = RandomValues.CreateNumberAsString(10),
                    LockoutEnabled = false,
                    HumanBeing = false,
                };

                result = await service.Http.User_CreateV1(rop.token, create);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<UserModel>().Should().BeAssignableTo<UserModel>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_DeleteV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_DeleteV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_DeleteV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                testUser.Immutable = true;

                await uow.UserRepo.UpdateAsync(testUser);
                await uow.CommitAsync();

                result = await service.Http.User_DeleteV1(rop.token, testUser.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_DeleteV1(rop.token, testUser.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = (await uow.UserRepo.GetAsync(x => x.Id == testUser.Id)).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                var result = await service.Http.User_DeleteRefreshesV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                result = await service.Http.User_DeleteRefreshesV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                result = await service.Http.User_DeleteRefreshV1(rop.token, user.Id, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                var result = await service.Http.User_DeleteRefreshesV1(rop.token, user.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                var refresh = (await uow.RefreshRepo.GetAsync(x => x.UserId == user.Id
                    && x.RefreshValue == rt)).Single();

                result = await service.Http.User_DeleteRefreshV1(rop.token, user.Id, refresh.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();
                await new TestData(uow).CreateRandomAsync(3);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("email", "asc"));

                var result = await service.Http.User_GetV1(rop.token,
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
                count.Should().Be(await uow.UserRepo.CountAsync());

                result = await service.Http.User_GetV1(rop.token, list.First().Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<UserModel>().Should().BeAssignableTo<UserModel>();

                result = await service.Http.User_GetV1(rop.token, list.First().Email.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<UserModel>().Should().BeAssignableTo<UserModel>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClaims_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_GetClaimsV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClaimModel>>();
                ok.Count().Should().Be((await uow.UserRepo.GetClaimsAsync(user.Id)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClients_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_GetClientsV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClientModel>>();
                ok.Count().Should().Be((await uow.UserRepo.GetClientsAsync(user.Id)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetLogins_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_GetLoginsV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<LoginModel>>();
                ok.Count().Should().Be((await uow.UserRepo.GetLoginsAsync(user.Id)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                var result = await service.Http.User_GetRefreshesV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<IEnumerable<RefreshModel>>().Should().BeAssignableTo<IEnumerable<RefreshModel>>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRoles_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_GetRolesV1(rop.token, user.Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<RoleModel>>();
                ok.Count().Should().Be((await uow.UserRepo.GetRolesAsync(user.Id)).Count());
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
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var login = await uow.LoginRepo.CreateAsync(
                    new LoginCreate()
                    {
                        Name = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestLogin,
                        LoginKey = Constants.ApiUnitTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    });

                await uow.CommitAsync();

                var add = await uow.UserRepo.AddToLoginAsync(testUser, login);
                add.Should().BeTrue();

                await uow.CommitAsync();

                var result = await service.Http.User_RemoveLoginV1(rop.token, testUser.Id, login.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await uow.UserRepo.IsInLoginAsync(testUser.Id, login.Id);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_RemovePasswordV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_RemovePasswordV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_RemovePasswordV1(rop.token, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_RemovePasswordV1(rop.token, testUser.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await uow.UserRepo.IsPasswordSetAsync(testUser.Id);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_SetPasswordV1(rop.token, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var model = new UserAddPassword()
                {
                    UserId = testUser.Id,
                    NewPassword = Constants.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Constants.ApiUnitTestUserPassNew
                };

                var result = await service.Http.User_SetPasswordV1(rop.token, testUser.Id, model);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = await uow.UserRepo.CheckPasswordAsync(testUser.Id, model.NewPassword);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_UpdateV1(rop.token, new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_UpdateV1(rop.token, new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_UpdateV1(rop.token, new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                testUser.FirstName += "(Updated)";

                var result = await service.Http.User_UpdateV1(rop.token, uow.Mapper.Map<UserModel>(testUser));
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                var check = ok.ToObject<UserModel>();
                check.FirstName.Should().Be(testUser.FirstName);
            }
        }
    }
}
