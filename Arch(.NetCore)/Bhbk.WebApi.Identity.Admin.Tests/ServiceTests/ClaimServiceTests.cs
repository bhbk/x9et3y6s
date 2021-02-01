using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
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
    public class ClaimServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public ClaimServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_ClaimV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var result = await service.Endpoints.Claim_CreateV1(Base64.CreateString(8), new ClaimV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Claim_CreateV1(rop.RawData, new ClaimV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.Claim_CreateV1(rop.RawData, new ClaimV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Claim_CreateV1(
                    new ClaimV1()
                    {
                        IssuerId = issuer.Id,
                        Type = TestDefaultConstants.ClaimName + "-" + Base64.CreateString(4),
                        Value = Base64.CreateString(8),
                    });
                result.Should().BeAssignableTo<ClaimV1>();

                var check = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Id == result.Id).ToLambda())
                    .Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var result = await service.Endpoints.Claim_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Claim_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.Claim_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateClaims();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClaim = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                    .Single();
                testClaim.IsDeletable = false;

                uow.Claims.Update(testClaim);
                uow.Commit();

                var result = await service.Endpoints.Claim_DeleteV1(rop.RawData, testClaim.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateClaims();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClaim = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                    .Single();

                var result = await service.Claim_DeleteV1(testClaim.Id);
                result.Should().BeTrue();

                var check = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Id == testClaim.Id).ToLambda())
                    .Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateClaims();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Normal).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClaim = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                    .Single();

                var result = await service.Claim_GetV1(testClaim.Id.ToString());
                result.Should().BeAssignableTo<ClaimV1>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateClaims();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Normal).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 1;
                var state = new DataStateV1()
                {
                    Sort = new List<IDataStateSort>() 
                    {
                        new DataStateV1Sort() { Field = "type", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = await service.Claim_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.Claims.Count());
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var result = await service.Endpoints.Claim_UpdateV1(Base64.CreateString(8), new ClaimV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Claim_UpdateV1(rop.RawData, new ClaimV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Endpoints.Claim_UpdateV1(rop.RawData, new ClaimV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateClaims();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testClaim = uow.Claims.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                    .Single();
                testClaim.Value += "(Updated)";

                var result = await service.Claim_UpdateV1(mapper.Map<ClaimV1>(testClaim));
                result.Should().BeAssignableTo<ClaimV1>();
                result.Value.Should().Be(testClaim.Value);
            }
        }
    }
}
