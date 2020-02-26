﻿using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Data.EFCore.Services;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
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
    public class IssuerServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public IssuerServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_IssuerV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Issuer_CreateV1(Base64.CreateString(8), new IssuerV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.Issuer_CreateV1(rop.RawData, new IssuerV1());
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

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.Issuer_CreateV1(rop.RawData, new IssuerV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Issuer_CreateV1(
                    new IssuerV1()
                    {
                        Name = Base64.CreateString(4) + "-" + Constants.ApiTestIssuer,
                        IssuerKey = Base64.CreateString(32),
                        Enabled = true,
                    });
                result.Should().BeAssignableTo<IssuerV1>();

                var check = uow.Issuers.Get(x => x.Id == result.Id).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Issuer_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.Issuer_DeleteV1(rop.RawData, Guid.NewGuid());
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

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.Issuer_DeleteV1(rop.RawData, Guid.NewGuid());
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

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testIssuer = uow.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();
                testIssuer.Immutable = true;

                uow.Issuers.Update(testIssuer);
                uow.Commit();

                var result = await service.Http.Issuer_DeleteV1(rop.RawData, testIssuer.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testIssuer = uow.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();

                var result = await service.Issuer_DeleteV1(testIssuer.Id);
                result.Should().BeTrue();

                var check = uow.Issuers.Get(x => x.Id == testIssuer.Id).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClient = uow.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();

                var result = await service.Issuer_GetV1(testClient.Id.ToString());
                result.Should().BeAssignableTo<IssuerV1>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

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
                        new PageStateTypeCSort() { Field = "name", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = await service.Issuer_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.Issuers.Count());
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_GetKeys_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Issuer_GetKeysV1(Base64.CreateString(8), new List<string> { } );
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.Issuer_GetKeysV1(rop.RawData, new List<string> { string.Empty });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_GetKeys_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceApi).Single();

                var cc_claims = uow.Audiences.GenerateAccessClaims(issuer, audience);
                service.AccessToken = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], audience.Name, cc_claims);

                var testIssuer = uow.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();
                var testList = new List<string> { issuer.Id.ToString(), testIssuer.Name };

                var result = await service.Issuer_GetKeysV1(testList);
                result.Should().BeAssignableTo<Dictionary<Guid, string>>();
                result.Count().Should().Be(testList.Count());
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Issuer_UpdateV1(Base64.CreateString(8), new IssuerV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Http.Issuer_UpdateV1(rop.RawData, new IssuerV1());
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

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Http.Issuer_UpdateV1(rop.RawData, new IssuerV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_IssuerV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testIssuer = uow.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();
                testIssuer.Description += "(Updated)";

                var result = await service.Issuer_UpdateV1(mapper.Map<IssuerV1>(testIssuer));
                result.Should().BeAssignableTo<IssuerV1>();
                result.Description.Should().Be(testIssuer.Description);
            }
        }
    }
}
