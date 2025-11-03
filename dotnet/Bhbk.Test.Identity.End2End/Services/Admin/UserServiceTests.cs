using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
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
using Bhbk.Test.Identity.Integration.TestingTools;

namespace Bhbk.Test.Identity.End2End.Services.Admin
{
    public class UserServiceTests : IClassFixture<BaseAdminServiceTests>
    {
        private readonly BaseAdminServiceTests _factory;

        public UserServiceTests(BaseAdminServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_UserV1_GetActivity_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateAudienceSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testActivity = uow.UserActivities.Get().FirstOrDefault();
                if (testActivity == null)
                {
                    uow.UserActivities.Post(new tbl_UserActivity
                    {
                        UserId = user.Id,
                        LoginType = "Password",
                        LoginOutcome = "Success",
                    });
                    uow.Commit();
                    testActivity = uow.UserActivities.Get().First();
                }

                var result = await service.Activity_GetV1(testActivity.Id.ToString());
                result.Should().BeAssignableTo<UserActivityV1>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddToClaim_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();
                var testClaim = uow.Claims.Post(
                    map.Map<tbl_Claim>(new ClaimV1()
                    {
                        IssuerId = seed.Issuer.Id,
                        Subject = Base64.CreateString(4),
                        Type = Base64.CreateString(4) + "-test-claim",
                        Value = Base64.CreateString(8),
                        ValueType = "String",
                        IsDeletable = false,
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
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();
                var testLogin = uow.LoginProviders.Post(
                    map.Map<tbl_LoginProvider>(new LoginProviderV1()
                    {
                        Name = Base64.CreateString(4) + "-test-login",
                        ProviderKey = Base64.CreateString(8),
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                uow.Commit();

                var result = await service.User_AddToLoginProviderV1(testUser.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInLoginProvider(testUser, testLogin);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddToRole_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();
                var testRole = uow.Roles.Post(
                    map.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = audience.Id,
                        Name = Base64.CreateString(4) + "-test-role",
                        IsEnabled = true,
                        IsDeletable = false,
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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var result = await service.Endpoints.User_CreateV1(Base64.CreateString(8), new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = seed.Users.Values.First();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.User_CreateV1(rop.RawData, new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.User_CreateV1(rop.RawData, new UserV1());
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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var address = Base64.CreateString(4) + "-test@example.com";

                var result = await service.User_CreateV1NoConfirm(
                    new UserV1()
                    {
                        UserName = address,
                        Email = address,
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        PhoneNumber = NumberAs.CreateString(11),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                    });
                result.Should().BeAssignableTo<UserV1>();

                var check = uow.Users.Get(x => x.Id == result.Id).Any();
                check.Should().BeTrue();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var address = Base64.CreateString(4) + "-test2@example.com";

                var result = await service.User_CreateV1(
                    new UserV1()
                    {
                        UserName = address,
                        Email = address,
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        PhoneNumber = NumberAs.CreateString(11),
                        IsLockedOut = false,
                        IsHumanBeing = false,
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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var result = await service.Endpoints.User_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.User_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();
                testUser.IsDeletable = false;

                uow.Users.Put(testUser);
                uow.Commit();

                var result = await service.Endpoints.User_DeleteV1(rop.RawData, testUser.Id);
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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                /* no using - the test deletes the user */
                var seed = ScenarioMother.CreateUserSeed(uow);
                var testUserId = seed.Users.Values.First().Id;

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_DeleteV1(testUserId);
                result.Should().BeTrue();

                var check = uow.Users.Get(x => x.Id == testUserId).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Post(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = ConsumerType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFrom = rt.ValidFrom,
                        ValidTo = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.User_DeleteRefreshesV1(rop.RawData, user.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Post(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = ConsumerType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFrom = rt.ValidFrom,
                        ValidTo = rt.ValidTo,
                    }));
                uow.Commit();

                var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                    .Where(x => x.UserId == user.Id && x.RefreshValue == rt.RawData).ToLambda()).Single();

                var result = await service.Endpoints.User_DeleteRefreshV1(rop.RawData, user.Id, refresh.Id);
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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);
                var testUser = seed.Users.Values.First();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetV1(testUser.Id.ToString());
                result.Should().BeAssignableTo<UserV1>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 2;
                var state = new PagerState()
                {
                    Sort = new List<IDataStateSort>()
                    {
                        new PagerStateSort() { Field = "userName", Dir = "asc" }
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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetClaimsV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<ClaimV1>>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetClients_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetAudiencesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<AudienceV1>>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetLogins_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetLoginProvidersV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<LoginProviderV1>>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                for (int i = 0; i < 3; i++)
                {
                    var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                    var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rt_claims);

                    uow.Refreshes.Post(
                        map.Map<tbl_Refresh>(new RefreshV1()
                        {
                            IssuerId = issuer.Id,
                            UserId = user.Id,
                            RefreshType = ConsumerType.User.ToString(),
                            RefreshValue = rt.RawData,
                            ValidFrom = rt.ValidFrom,
                            ValidTo = rt.ValidTo,
                        }));
                }
                uow.Commit();

                var result = await service.User_GetRefreshesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RefreshV1>>();
                result.Count().Should().Be(uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                    .Where(x => x.UserId == user.Id).ToLambda()).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetRoles_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.User_GetRolesV1(user.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RoleV1>>();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromClaim_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();
                var testClaim = uow.Claims.Post(
                    map.Map<tbl_Claim>(new ClaimV1()
                    {
                        IssuerId = seed.Issuer.Id,
                        Subject = Base64.CreateString(4),
                        Type = Base64.CreateString(4) + "-test-claim",
                        Value = Base64.CreateString(8),
                        ValueType = "String",
                        IsDeletable = true,
                    }));
                uow.Commit();

                uow.Users.AddClaim(new tbl_UserClaim { UserId = testUser.Id, ClaimId = testClaim.Id });
                uow.Commit();

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
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();
                var testLogin = uow.LoginProviders.Post(
                    map.Map<tbl_LoginProvider>(new LoginProviderV1()
                    {
                        Name = Base64.CreateString(4) + "-test-login",
                        ProviderKey = Base64.CreateString(8),
                        IsEnabled = true,
                        IsDeletable = true,
                    }));
                uow.Commit();

                uow.Users.AddLoginProvider(new tbl_UserLoginProvider { UserId = testUser.Id, LoginProviderId = testLogin.Id });
                uow.Commit();

                var result = await service.User_RemoveFromLoginProviderV1(testUser.Id, testLogin.Id);
                result.Should().BeTrue();

                var check = uow.Users.IsInLoginProvider(testUser, testLogin);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemoveFromRole_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateAudienceSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = seed.Audiences.Values.First();
                var testUser = uow.Users.Post(
                    map.Map<tbl_User>(new UserV1()
                    {
                        UserName = Base64.CreateString(4) + "-test@example.com",
                        Email = Base64.CreateString(4) + "-test@example.com",
                        FirstName = "Test",
                        LastName = "User",
                        IsHumanBeing = true,
                        IsDeletable = true,
                    }));
                uow.Commit();

                var testRole = uow.Roles.Post(
                    map.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = testAudience.Id,
                        Name = Base64.CreateString(4) + "-test-role",
                        IsEnabled = true,
                        IsDeletable = true,
                    }));
                uow.Commit();

                uow.Users.AddRole(new tbl_UserRole { UserId = testUser.Id, RoleId = testRole.Id });
                uow.Commit();

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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();

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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = seed.Users.Values.First();
                var newPassword = Base64.CreateString(12);
                var testUserPassword = new PasswordAddV1()
                {
                    EntityId = testUser.Id,
                    NewPassword = newPassword,
                    NewPasswordConfirm = newPassword
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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var result = await service.Endpoints.User_UpdateV1(Base64.CreateString(8), new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = seed.Users.Values.First();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.User_UpdateV1(rop.RawData, new UserV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.User_UpdateV1(rop.RawData, new UserV1());
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
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.GetAsNoTracking(x => x.Id == seed.Users.Values.First().Id).Single();
                testUser.FirstName += "(Updated)";

                var result = await service.User_UpdateV1(map.Map<UserV1>(testUser));
                result.Should().BeAssignableTo<UserV1>();
                result.FirstName.Should().Be(testUser.FirstName);
            }
        }
    }
}
