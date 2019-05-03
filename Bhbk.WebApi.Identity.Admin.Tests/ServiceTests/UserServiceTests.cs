using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    public class UserServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public UserServiceTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Admin_UserV1_AddToClaim_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testIssuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var testClaim = await uow.ClaimRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = testIssuer.Id,
                        Type = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestClaim,
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    }));

                await uow.CommitAsync();

                var result = service.User_AddClaimV1(testUser.Id, testClaim.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsInClaimAsync(testUser.Id, testClaim.Id);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.User_AddPasswordV1(RandomValues.CreateBase64String(8), Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_AddPasswordV1(rop.RawData, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_AddPasswordV1(rop.RawData, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                (await uow.UserRepo.RemovePasswordAsync(testUser.Id)).Should().BeTrue();

                var result = service.User_AddPasswordV1(testUser.Id,
                    new UserAddPassword()
                    {
                        UserId = testUser.Id,
                        NewPassword = Constants.ApiUnitTestUserPassNew,
                        NewPasswordConfirm = Constants.ApiUnitTestUserPassNew,
                    });
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsPasswordSetAsync(testUser.Id);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddToLogin_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var testLogin = await uow.LoginRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestLogin,
                        LoginKey = Constants.ApiUnitTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await uow.CommitAsync();

                var result = service.User_AddLoginV1(testUser.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsInLoginAsync(testUser.Id, testLogin.Id);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.User_CreateV1(RandomValues.CreateBase64String(8), new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_CreateV1(rop.RawData, new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_CreateV1(rop.RawData, new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testIssuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = service.User_CreateV1NoConfirm(
                    new UserCreate()
                    {
                        Email = RandomValues.CreateBase64String(4) + "-" + testUser.Email,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        PhoneNumber = RandomValues.CreateNumberAsString(10),
                        LockoutEnabled = false,
                        HumanBeing = true,
                    });
                result.Should().BeAssignableTo<UserModel>();

                var check = (await uow.UserRepo.GetAsync(x => x.Id == result.Id)).Any();
                check.Should().BeTrue();

                result = service.User_CreateV1(
                    new UserCreate()
                    {
                        IssuerId = testIssuer.Id,
                        Email = RandomValues.CreateBase64String(4) + "-" + testUser.Email,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        PhoneNumber = RandomValues.CreateNumberAsString(10),
                        LockoutEnabled = false,
                        HumanBeing = false,
                    });
                result.Should().BeAssignableTo<UserModel>();

                check = (await uow.UserRepo.GetAsync(x => x.Id == result.Id)).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.User_DeleteV1(RandomValues.CreateBase64String(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                testUser.Immutable = true;

                await uow.UserRepo.UpdateAsync(testUser);
                await uow.CommitAsync();

                result = await service.Http.User_DeleteV1(rop.RawData, testUser.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = service.User_DeleteV1(testUser.Id);
                result.Should().BeTrue();

                var check = (await uow.UserRepo.GetAsync(x => x.Id == testUser.Id)).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.User_DeleteRefreshesV1(RandomValues.CreateBase64String(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_DeleteRefreshesV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                await JwtFactory.UserRefreshV2(uow, issuer, user);
                await uow.CommitAsync();

                result = await service.Http.User_DeleteRefreshesV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                result = await service.Http.User_DeleteRefreshV1(rop.RawData, user.Id, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                await uow.CommitAsync();

                var result = await service.Http.User_DeleteRefreshesV1(rop.RawData, user.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                await uow.CommitAsync();

                var refresh = (await uow.RefreshRepo.GetAsync(x => x.UserId == user.Id
                    && x.RefreshValue == rt.RawData)).Single();

                result = await service.Http.User_DeleteRefreshV1(rop.RawData, user.Id, refresh.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Get_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();
                await new TestData(uow).CreateRandomAsync(3);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("email", "asc"));

                var multiple = service.User_GetV1(
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                        Skip = 1,
                        Take = take,
                    });
                multiple.Item1.Should().Be(await uow.UserRepo.CountAsync());
                multiple.Item2.Should().BeAssignableTo<IEnumerable<UserModel>>();
                multiple.Item2.Count().Should().Be(take);

                var single = service.User_GetV1(multiple.Item2.First().Id.ToString());
                single.Should().BeAssignableTo<UserModel>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetMOTDs_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();
                await new TestData(uow).CreateRandomAsync(3);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("author", "asc"));

                var result = service.User_GetMOTDsV1(
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                        Skip = 1,
                        Take = take,
                    });
                result.Item1.Should().Be(await uow.UserRepo.CountMOTDAsync());
                result.Item2.Should().BeAssignableTo<IEnumerable<MotDType1Model>>();
                result.Item2.Count().Should().Be(take);
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClaims_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = service.User_GetClaimsV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<ClaimModel>>();
                result.Count().Should().Be((await uow.UserRepo.GetClaimsAsync(user.Id)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClients_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = service.User_GetClientsV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<ClientModel>>();
                result.Count().Should().Be((await uow.UserRepo.GetClientsAsync(user.Id)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetLogins_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = service.User_GetLoginsV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<LoginModel>>();
                result.Count().Should().Be((await uow.UserRepo.GetLoginsAsync(user.Id)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRefreshes_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var rt = await JwtFactory.UserRefreshV2(uow, issuer, user);

                var result = service.User_GetRefreshesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RefreshModel>>();
                result.Count().Should().Be((await uow.RefreshRepo.GetAsync(x => x.UserId == user.Id)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRoles_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = service.User_GetRolesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RoleModel>>();
                result.Count().Should().Be((await uow.UserRepo.GetRolesAsync(user.Id)).Count());
            }
        }

        [Fact(Skip = "NotImplemented")]
        public void Admin_UserV1_RemoveFromClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromLogin_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var testLogin = await uow.LoginRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestLogin,
                        LoginKey = Constants.ApiUnitTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                var add = await uow.UserRepo.AddToLoginAsync(testUser, testLogin);
                add.Should().BeTrue();

                await uow.CommitAsync();

                var result = service.User_RemoveLoginV1(testUser.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsInLoginAsync(testUser.Id, testLogin.Id);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.User_RemovePasswordV1(RandomValues.CreateBase64String(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_RemovePasswordV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_RemovePasswordV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = service.User_RemovePasswordV1(testUser.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsPasswordSetAsync(testUser.Id);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.User_SetPasswordV1(RandomValues.CreateBase64String(8), Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_SetPasswordV1(rop.RawData, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_SetPasswordV1(rop.RawData, Guid.NewGuid(), new UserAddPassword());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var testUserPassword = new UserAddPassword()
                {
                    UserId = testUser.Id,
                    NewPassword = Constants.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Constants.ApiUnitTestUserPassNew
                };

                var result = service.User_SetPasswordV1(testUser.Id, testUserPassword);
                result.Should().BeTrue();

                var check = await uow.UserRepo.CheckPasswordAsync(testUser.Id, testUserPassword.NewPassword);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.User_UpdateV1(RandomValues.CreateBase64String(8), new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_UpdateV1(rop.RawData, new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.User_UpdateV1(rop.RawData, new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                testUser.FirstName += "(Updated)";

                var result = service.User_UpdateV1(uow.Mapper.Map<UserModel>(testUser));
                result.Should().BeAssignableTo<UserModel>();
                result.FirstName.Should().Be(testUser.FirstName);
            }
        }
    }
}
