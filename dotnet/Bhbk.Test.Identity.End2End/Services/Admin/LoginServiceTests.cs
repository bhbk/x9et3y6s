using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
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
using Bhbk.Test.Identity.Integration.TestingTools;

namespace Bhbk.Test.Identity.End2End.Services.Admin
{
    public class LoginServiceTests : IClassFixture<BaseAdminServiceTests>
    {
        private readonly BaseAdminServiceTests _factory;

        public LoginServiceTests(BaseAdminServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_LoginV1_Create_Fail()
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

                var result = await service.Endpoints.LoginProvider_CreateV1(Base64.CreateString(8), new LoginProviderV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = seed.Users.Values.First();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.LoginProvider_CreateV1(rop.RawData, new LoginProviderV1());
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

                var result = await service.Endpoints.LoginProvider_CreateV1(rop.RawData, new LoginProviderV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_LoginV1_Create_Success()
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

                var result = await service.LoginProvider_CreateV1(
                    new LoginProviderV1()
                    {
                        Name = Base64.CreateString(4) + "-test-login",
                        ProviderKey = Base64.CreateString(8),
                        IsEnabled = true,
                        IsDeletable = false
                    });
                result.Should().BeAssignableTo<LoginProviderV1>();

                var check = uow.LoginProviders.Get(x => x.Id == result.Id).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_LoginV1_Delete_Fail()
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

                var result = await service.Endpoints.LoginProvider_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = seed.Users.Values.First();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.LoginProvider_DeleteV1(rop.RawData, Guid.NewGuid());
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

                var result = await service.Endpoints.LoginProvider_DeleteV1(rop.RawData, Guid.NewGuid());
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

                using var seed = ScenarioMother.CreateLoginProviderSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testLogin = seed.LoginProviders.Values.First();
                testLogin.IsDeletable = false;

                uow.LoginProviders.Put(testLogin);
                uow.Commit();

                var result = await service.Endpoints.LoginProvider_DeleteV1(rop.RawData, testLogin.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_LoginV1_Delete_Success()
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

                /* no using - the test deletes the login provider */
                var seed = ScenarioMother.CreateLoginProviderSeed(uow);
                var testLoginId = seed.LoginProviders.Values.First().Id;

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.LoginProvider_DeleteV1(testLoginId);
                result.Should().BeTrue();

                var check = uow.LoginProviders.Get(x => x.Id == testLoginId).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_LoginV1_Get_Success()
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

                using var seed = ScenarioMother.CreateLoginProviderSeed(uow);
                var testLogin = seed.LoginProviders.Values.First();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.LoginProvider_GetV1(testLogin.Id.ToString());
                result.Should().BeAssignableTo<LoginProviderV1>();
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

                using var seed = ScenarioMother.CreateLoginProviderSeed(uow);

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
                        new PagerStateSort() { Field = "name", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = await service.LoginProvider_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.LoginProviders.Count());
            }
        }

        [Fact]
        public async Task Admin_LoginV1_Update_Fail()
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

                var result = await service.Endpoints.LoginProvider_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                using var seed = ScenarioMother.CreateUserSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = seed.Users.Values.First();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.LoginProvider_UpdateV1(rop.RawData, new LoginProviderV1());
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

                var result = await service.Endpoints.LoginProvider_UpdateV1(rop.RawData, new LoginProviderV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_LoginV1_Update_Success()
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

                using var seed = ScenarioMother.CreateLoginProviderSeed(uow);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameIdentity).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testLogin = uow.LoginProviders.GetAsNoTracking(x => x.Id == seed.LoginProviders.Values.First().Id).Single();
                testLogin.Description += "(Updated)";

                var result = await service.LoginProvider_UpdateV1(map.Map<LoginProviderV1>(testLogin));
                result.Should().BeAssignableTo<LoginProviderV1>();
                result.Description.Should().Be(testLogin.Description);
            }
        }
    }
}
