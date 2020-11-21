using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
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

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class AudienceServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public AudienceServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_AudienceV1_AddToRole_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var testRole = uow.Roles.Create(
                    mapper.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = audience.Id,
                        Name = Base64.CreateString(4) + "-" + Constants.TestRole,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                uow.Commit();

                var result = await service.Audience_AddToRoleV1(testAudience.Id, testRole.Id);
                result.Should().BeTrue();

                var check = uow.Audiences.IsInRole(testAudience, testRole);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var result = await service.Endpoints.Audience_CreateV1(Base64.CreateString(8), new AudienceV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Audience_CreateV1(rop.RawData, new AudienceV1());
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
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.Audience_CreateV1(rop.RawData, new AudienceV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Audience_CreateV1(
                    new AudienceV1()
                    {
                        IssuerId = issuer.Id,
                        Name = Base64.CreateString(4) + "-" + audience.Name,
                        IsEnabled = true,
                    });
                result.Should().BeAssignableTo<AudienceV1>();

                var check = uow.Audiences.Get(x => x.Id == result.Id).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var result = await service.Endpoints.Audience_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Audience_DeleteV1(rop.RawData, Guid.NewGuid());
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
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.Audience_DeleteV1(rop.RawData, Guid.NewGuid());
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
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                testAudience.IsDeletable = true;

                uow.Audiences.Update(testAudience);
                uow.Commit();

                var result = await service.Endpoints.Audience_DeleteV1(rop.RawData, testAudience.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();

                var result = await service.Audience_DeleteV1(testAudience.Id);
                result.Should().BeTrue();

                var check = uow.Audiences.Get(x => x.Id == testAudience.Id).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                for (int i = 0; i < 3; i++)
                {
                    var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                    var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], audience.Name, rt_claims);

                    uow.Refreshes.Create(
                        mapper.Map<tbl_Refresh>(new RefreshV1()
                        {
                            IssuerId = issuer.Id,
                            AudienceId = audience.Id,
                            RefreshType = RefreshType.Client.ToString(),
                            RefreshValue = rt.RawData,
                            ValidFromUtc = rt.ValidFrom,
                            ValidToUtc = rt.ValidTo,
                        }));
                }
                uow.Commit();

                var result = await service.Audience_DeleteRefreshesV1(audience.Id);
                result.Should().BeTrue();

                var check = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                    .Where(x => x.AudienceId == audience.Id).ToLambda()).Any();
                check.Should().BeFalse();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                    .Where(x => x.AudienceId == audience.Id).ToLambda()).Single();
                var result = await service.Audience_DeleteRefreshV1(audience.Id, refresh.Id);
                result.Should().BeTrue();

                var check = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                    .Where(x => x.Id == refresh.Id).ToLambda()).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Normal).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();

                var result = await service.Audience_GetV1(testAudience.Id.ToString());
                result.Should().BeAssignableTo<AudienceV1>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Normal).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 2;
                var state = new DataStateV1()
                {
                    Sort = new List<IDataStateSort>()
                    {
                        new DataStateV1Sort() { Field = "name", Dir = "asc" },
                    },
                    Skip = 0,
                    Take = take
                };

                var result = await service.Audience_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.Audiences.Count());
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                for (int i = 0; i < 3; i++)
                {
                    var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                    var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], audience.Name, rt_claims);

                    uow.Refreshes.Create(
                        mapper.Map<tbl_Refresh>(new RefreshV1()
                        {
                            IssuerId = issuer.Id,
                            AudienceId = audience.Id,
                            RefreshType = RefreshType.Client.ToString(),
                            RefreshValue = rt.RawData,
                            ValidFromUtc = rt.ValidFrom,
                            ValidToUtc = rt.ValidTo,
                        }));
                }
                uow.Commit();

                var result = await service.Audience_GetRefreshesV1(audience.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RefreshV1>>();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_RemoveFromRole_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var testRole = uow.Roles.Get(x => x.Name == Constants.TestRole).Single();

                var result = await service.Audience_RemoveFromRoleV1(testAudience.Id, testRole.Id);
                result.Should().BeTrue();

                var check = uow.Audiences.IsInRole(testAudience, testRole);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_RemovePassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();

                var result = await service.Audience_RemovePasswordV1(testAudience.Id);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var testAudiencePassword = new PasswordAddV1()
                {
                    EntityId = testAudience.Id,
                    NewPassword = Constants.TestAudiencePassNew,
                    NewPasswordConfirm = Constants.TestAudiencePassNew
                };

                var result = await service.Audience_SetPasswordV1(testAudience.Id, testAudiencePassword);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var result = await service.Endpoints.Audience_UpdateV1(Base64.CreateString(8), new AudienceV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Audience_UpdateV1(rop.RawData, new AudienceV1());
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
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.Audience_UpdateV1(rop.RawData, new AudienceV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_AudienceV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testAudience = uow.Audiences.GetAsNoTracking(x => x.Name == Constants.TestAudience).Single();
                testAudience.Description += "(Updated)";

                var result = await service.Audience_UpdateV1(mapper.Map<AudienceV1>(testAudience));
                result.Should().BeAssignableTo<AudienceV1>();
                result.Description.Should().Be(testAudience.Description);
            }
        }
    }
}
