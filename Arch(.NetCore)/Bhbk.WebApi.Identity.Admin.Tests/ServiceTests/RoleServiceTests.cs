using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
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
    public class RoleServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public RoleServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_RoleV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Role_CreateV1(Base64.CreateString(8), new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                result = await service.Http.Role_CreateV1(rop.RawData, new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var result = await service.Http.Role_CreateV1(rop.RawData, new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var result = await service.Role_CreateV1(
                    new RoleCreate()
                    {
                        AudienceId = client.Id,
                        Name = Base64.CreateString(4) + "-" + FakeConstants.ApiTestRole,
                        Enabled = true,
                        Immutable = false
                    });
                result.Should().BeAssignableTo<RoleModel>();

                var check = uow.Roles.Get(x => x.Id == result.Id).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Role_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                result = await service.Http.Role_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var result = await service.Http.Role_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var testRole = uow.Roles.Get(x => x.Name == FakeConstants.ApiTestRole).Single();
                testRole.Immutable = true;

                uow.Roles.Update(testRole);
                uow.Commit();

                var result = await service.Http.Role_DeleteV1(rop.RawData, testRole.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var testRole = uow.Roles.Get(x => x.Name == FakeConstants.ApiTestRole).Single();

                var result = await service.Role_DeleteV1(testRole.Id);
                result.Should().BeTrue();

                var check = uow.Roles.Get(x => x.Id == testRole.Id).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var testRole = uow.Roles.Get(x => x.Name == FakeConstants.ApiTestRole).Single();

                var result = await service.Role_GetV1(testRole.Id.ToString());
                result.Should().BeAssignableTo<RoleModel>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create(3);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

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

                var result = await service.Role_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.Roles.Count());
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Role_UpdateV1(Base64.CreateString(8), new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                result = await service.Http.Role_UpdateV1(rop.RawData, new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var result = await service.Http.Role_UpdateV1(rop.RawData, new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var testRole = uow.Roles.Get(x => x.Name == FakeConstants.ApiTestRole).Single();
                testRole.Description += "(Updated)";

                var result = await service.Role_UpdateV1(mapper.Map<RoleModel>(testRole));
                result.Should().BeAssignableTo<RoleModel>();
                result.Description.Should().Be(testRole.Description);
            }
        }
    }
}
