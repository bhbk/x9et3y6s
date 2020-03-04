using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Bhbk.Lib.DataState.Models.PageStateTypeC;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class UserServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public UserServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_UserV1_AddToClaim_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                var testClaim = uow.Claims.Create(
                    mapper.Map<tbl_Claims>(new ClaimV1()
                    {
                        IssuerId = issuer.Id,
                        Type = Base64.CreateString(4) + "-" + Constants.ApiTestClaim,
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));

                uow.Commit();

                var result = await service.User_AddToClaimV1(testUser.Id, testClaim.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInClaim(testUser, testClaim);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddToLogin_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                var testLogin = uow.Logins.Create(
                    mapper.Map<tbl_Logins>(new LoginV1()
                    {
                        Name = Base64.CreateString(4) + "-" + Constants.ApiTestLogin,
                        LoginKey = Constants.ApiTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                uow.Commit();

                var result = await service.User_AddToLoginV1(testUser.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInLogin(testUser, testLogin);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddToRole_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                var testRole = uow.Roles.Create(
                    mapper.Map<tbl_Roles>(new RoleV1()
                    {
                        AudienceId = audience.Id,
                        Name = Base64.CreateString(4) + "-" + Constants.ApiTestLogin,
                        Enabled = true,
                        Immutable = false,
                    }));

                uow.Commit();

                var result = await service.User_AddToRoleV1(testUser.Id, testRole.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInRole(testUser, testRole);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var result = await service.Http.User_CreateV1(Base64.CreateString(8), new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.User_CreateV1(rop.RawData, new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.User_CreateV1(rop.RawData, new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_CreateV1NoConfirm(
                    new UserV1()
                    {
                        Email = Base64.CreateString(4) + "-" + Constants.ApiTestUser,
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        PhoneNumber = NumberAs.CreateString(10),
                        LockoutEnabled = false,
                        HumanBeing = true,
                    });
                result.Should().BeAssignableTo<UserV1>();

                var check = uow.Users.Get(x => x.Id == result.Id).Any();
                check.Should().BeTrue();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_CreateV1(
                    new UserV1()
                    {
                        IssuerId = issuer.Id,
                        Email = Base64.CreateString(4) + "-" + Constants.ApiTestUser,
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        PhoneNumber = NumberAs.CreateString(10),
                        LockoutEnabled = false,
                        HumanBeing = false,
                    });
                result.Should().BeAssignableTo<UserV1>();

                var check = uow.Users.Get(x => x.Id == result.Id).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var result = await service.Http.User_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.User_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                testUser.Immutable = true;

                uow.Users.Update(testUser);
                uow.Commit();

                var result = await service.Http.User_DeleteV1(rop.RawData, testUser.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var result = await service.User_DeleteV1(testUser.Id);
                result.Should().BeTrue();

                var check = uow.Users.Get(x => x.Id == testUser.Id).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refreshes>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Http.User_DeleteRefreshesV1(rop.RawData, user.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refreshes>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var refresh = uow.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.UserId == user.Id && x.RefreshValue == rt.RawData).ToLambda()).Single();

                var result = await service.Http.User_DeleteRefreshV1(rop.RawData, user.Id, refresh.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var result = await service.User_GetV1(testUser.Id.ToString());
                result.Should().BeAssignableTo<UserV1>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create(3);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 2;
                var state = new PageStateTypeC()
                {
                    Sort = new List<PageStateTypeCSort>()
                    {
                        new PageStateTypeCSort() { Field = "email", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = await service.User_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.Users.Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClaims_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetClaimsV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<ClaimV1>>();
                result.Count().Should().Be(uow.Claims.Count(new QueryExpression<tbl_Claims>()
                    .Where(x => x.tbl_UserClaims.Any(y => y.UserId == user.Id)).ToLambda()));
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClients_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetAudiencesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<AudienceV1>>();
                result.Count().Should().Be(uow.Audiences.Count(new QueryExpression<tbl_Audiences>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda()));
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetLogins_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetLoginsV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<LoginV1>>();
                result.Count().Should().Be(uow.Logins.Count(new QueryExpression<tbl_Logins>()
                    .Where(x => x.tbl_UserLogins.Any(y => y.UserId == user.Id)).ToLambda()));
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                for (int i = 0; i < 3; i++)
                {
                    var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                    var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                    uow.Refreshes.Create(
                        mapper.Map<tbl_Refreshes>(new RefreshV1()
                        {
                            IssuerId = issuer.Id,
                            UserId = user.Id,
                            RefreshType = RefreshType.User.ToString(),
                            RefreshValue = rt.RawData,
                            ValidFromUtc = rt.ValidFrom,
                            ValidToUtc = rt.ValidTo,
                        }));
                }
                uow.Commit();

                var result = await service.User_GetRefreshesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RefreshV1>>();
                result.Count().Should().Be(uow.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.UserId == user.Id).ToLambda()).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRoles_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetRolesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RoleV1>>();
                result.Count().Should().Be(uow.Roles.Count(new QueryExpression<tbl_Roles>()
                    .Where(x => x.tbl_UserRoles.Any(y => y.UserId == user.Id)).ToLambda()));
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromClaim_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                var testClaim = uow.Claims.Get(new QueryExpression<tbl_Claims>()
                    .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                    .Single();

                var result = await service.User_RemoveFromClaimV1(testUser.Id, testClaim.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInClaim(testUser, testClaim);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromLogin_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                var testLogin = uow.Logins.Get(x => x.Name == Constants.ApiTestLogin).Single();

                var result = await service.User_RemoveFromLoginV1(testUser.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInLogin(testUser, testLogin);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromRole_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                var testRole = uow.Roles.Get(x => x.Name == Constants.ApiTestRole).Single();

                var result = await service.User_RemoveFromRoleV1(testUser.Id, testRole.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInRole(testUser, testRole);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var result = await service.User_RemovePasswordV1(testUser.Id);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                var testUserPassword = new PasswordAddV1()
                {
                    EntityId = testUser.Id,
                    NewPassword = Constants.ApiTestUserPassNew,
                    NewPasswordConfirm = Constants.ApiTestUserPassNew
                };

                var result = await service.User_SetPasswordV1(testUser.Id, testUserPassword);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var result = await service.Http.User_UpdateV1(Base64.CreateString(8), new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.User_UpdateV1(rop.RawData, new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.User_UpdateV1(rop.RawData, new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
                testUser.FirstName += "(Updated)";

                var result = await service.User_UpdateV1(mapper.Map<UserV1>(testUser));
                result.Should().BeAssignableTo<UserV1>();
                result.FirstName.Should().Be(testUser.FirstName);
            }
        }
    }
}
