﻿using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
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
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class AudienceServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public AudienceServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_AudienceV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Audience_CreateV1(Base64.CreateString(8), new AudienceCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.Audience_CreateV1(rop.RawData, new AudienceCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.Audience_CreateV1(rop.RawData, new AudienceCreate());
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Audience_CreateV1(
                    new AudienceCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = Base64.CreateString(4) + "-" + audience.Name,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                    });
                result.Should().BeAssignableTo<AudienceModel>();

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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Audience_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.Audience_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.Audience_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClient = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                testClient.Immutable = true;

                uow.Audiences.Update(testClient);
                uow.Commit();

                var result = await service.Http.Audience_DeleteV1(rop.RawData, testClient.Id);
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClient = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();

                var result = await service.Audience_DeleteV1(testClient.Id);
                result.Should().BeTrue();

                var check = uow.Audiences.Get(x => x.Id == testClient.Id).Any();
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                for (int i = 0; i < 3; i++)
                {
                    var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                    var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], audience.Name, rt_claims);

                    uow.Refreshes.Create(
                        mapper.Map<tbl_Refreshes>(new RefreshCreate()
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

                var check = uow.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.AudienceId == audience.Id).ToLambda()).Any();
                check.Should().BeFalse();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var refresh = uow.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.AudienceId == audience.Id).ToLambda()).Single();
                var result = await service.Audience_DeleteRefreshV1(audience.Id, refresh.Id);
                result.Should().BeTrue();

                var check = uow.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClient = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();

                var result = await service.Audience_GetV1(testClient.Id.ToString());
                result.Should().BeAssignableTo<AudienceModel>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 2;
                var state = new PageStateTypeC()
                {
                    Sort = new List<PageStateTypeCSort>()
                    {
                        new PageStateTypeCSort() { Field = "name", Dir = "asc" },
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                for (int i = 0; i < 3; i++)
                {
                    var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                    var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], audience.Name, rt_claims);

                    uow.Refreshes.Create(
                        mapper.Map<tbl_Refreshes>(new RefreshCreate()
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
                result.Should().BeAssignableTo<IEnumerable<RefreshModel>>();
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClient = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var testClientPassword = new PasswordAddModel()
                {
                    EntityId = testClient.Id,
                    NewPassword = FakeConstants.ApiTestAudiencePassNew,
                    NewPasswordConfirm = FakeConstants.ApiTestAudiencePassNew
                };

                var result = await service.Audience_SetPasswordV1(testClient.Id, testClientPassword);
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Audience_UpdateV1(Base64.CreateString(8), new AudienceModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.Audience_UpdateV1(rop.RawData, new AudienceModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.Audience_UpdateV1(rop.RawData, new AudienceModel());
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
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClient = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                testClient.Description += "(Updated)";

                var result = await service.Audience_UpdateV1(mapper.Map<AudienceModel>(testClient));
                result.Should().BeAssignableTo<AudienceModel>();
                result.Description.Should().Be(testClient.Description);
            }
        }
    }
}
