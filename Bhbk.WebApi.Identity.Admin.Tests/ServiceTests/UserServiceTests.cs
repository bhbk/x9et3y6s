﻿using Bhbk.Lib.Core.Cryptography;
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
        public async Task Admin_UserV1_AddClaim_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testClaim = await uow.ClaimRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = issuer.Id,
                        Type = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestClaim,
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    }));

                uow.CommitAsync().Wait();

                var result = service.User_AddClaimV1(user.Id, testClaim.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsInClaimAsync(user.Id, testClaim.Id);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddLogin_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testLogin = await uow.LoginRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestLogin,
                        LoginKey = Constants.ApiUnitTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                uow.CommitAsync().Wait();

                var result = service.User_AddLoginV1(user.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsInLoginAsync(user.Id, testLogin.Id);
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

                var result = await service.Http.User_CreateV1(RandomValues.CreateBase64String(8), new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_CreateV1(rop.RawData, new UserCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_CreateV1(rop.RawData, new UserCreate());
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

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = service.User_CreateV1NoConfirm(
                    new UserCreate()
                    {
                        Email = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestUser,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        PhoneNumber = RandomValues.CreateNumberAsString(10),
                        LockoutEnabled = false,
                        HumanBeing = true,
                    });
                result.Should().BeAssignableTo<UserModel>();

                var check = (await uow.UserRepo.GetAsync(x => x.Id == result.Id)).Any();
                check.Should().BeTrue();
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testIssuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var result = service.User_CreateV1(
                    new UserCreate()
                    {
                        IssuerId = testIssuer.Id,
                        Email = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestUser,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        PhoneNumber = RandomValues.CreateNumberAsString(10),
                        LockoutEnabled = false,
                        HumanBeing = false,
                    });
                result.Should().BeAssignableTo<UserModel>();

                var check = (await uow.UserRepo.GetAsync(x => x.Id == result.Id)).Any();
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

                var result = await service.Http.User_DeleteV1(RandomValues.CreateBase64String(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                testUser.Immutable = true;

                uow.UserRepo.UpdateAsync(testUser).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.User_DeleteV1(rop.RawData, testUser.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                new TestData(uow).DestroyAsync().Wait();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

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
        public async Task Admin_UserV1_DeleteRefreshes_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                await JwtFactory.UserRefreshV2(uow, issuer, user);
                uow.CommitAsync().Wait();

                var result = await service.Http.User_DeleteRefreshesV1(rop.RawData, user.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var rt = await JwtFactory.UserRefreshV2(uow, issuer, user);
                uow.CommitAsync().Wait();

                var refresh = (await uow.RefreshRepo.GetAsync(x => x.UserId == user.Id
                    && x.RefreshValue == rt.RawData)).Single();

                var result = await service.Http.User_DeleteRefreshV1(rop.RawData, user.Id, refresh.Id);
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

                new TestData(uow).CreateRandomAsync(3).Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 2;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("email", "asc"));

                var result = service.User_GetV1(
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                        Skip = 1,
                        Take = take,
                    });
                result.Item1.Should().Be(await uow.UserRepo.CountAsync());
                result.Item2.Should().BeAssignableTo<IEnumerable<UserModel>>();
                result.Item2.Count().Should().Be(take);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = service.User_GetV1(testUser.Id.ToString());
                result.Should().BeAssignableTo<UserModel>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClaims_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

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
        public async Task Admin_UserV1_GetMOTDs_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateRandomAsync(3).Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 2;
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
        public async Task Admin_UserV1_GetRefreshes_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                for (int i = 0; i < 3; i++)
                    await JwtFactory.UserRefreshV2(uow, issuer, user);
                uow.CommitAsync().Wait();

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

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var testLogin = (await uow.LoginRepo.GetAsync(x => x.Name == Constants.ApiUnitTestLogin)).Single();

                var result = service.User_RemoveLoginV1(testUser.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsInLoginAsync(testUser.Id, testLogin.Id);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = service.User_RemovePasswordV1(testUser.Id);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

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
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var result = await service.Http.User_UpdateV1(RandomValues.CreateBase64String(8), new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_UpdateV1(rop.RawData, new UserModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.User_UpdateV1(rop.RawData, new UserModel());
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

                new TestData(uow).CreateAsync().Wait();

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
