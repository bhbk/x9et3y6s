using AutoFixture;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Test.Identity.End2End.Services.Admin
{
    public class DequeueServiceTests : IClassFixture<BaseAdminServiceTests>
    {
        private readonly BaseAdminServiceTests _factory;
        private readonly IFixture _fixture;

        public DequeueServiceTests(BaseAdminServiceTests factory)
        {
            _factory = factory;
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Alert_DequeueV1_Email_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var result = await service.Endpoints.Dequeue_EmailV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameAlert).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Dequeue_EmailV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Alert_DequeueV1_Email_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameAlert).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var email = uow.EmailQueue.Post(new tbl_EmailQueue
                {
                    FromEmail = _fixture.Create<string>() + "@test.com",
                    ToEmail = _fixture.Create<string>() + "@test.com",
                    Subject = _fixture.Create<string>(),
                    Body = _fixture.Create<string>(),
                    IsCancelled = false,
                });
                uow.Commit();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Dequeue_EmailV1(email.Id);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Alert_DequeueV1_Text_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var result = await service.Endpoints.Dequeue_TextV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameAlert).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Dequeue_TextV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Alert_DequeueV1_Text_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Seed.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Seed.AudienceNameAlert).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.Seed.UserNameAdmin).Single();

                var text = uow.TextQueue.Post(new tbl_TextQueue
                {
                    FromPhoneNumber = NumberAs.CreateString(11),
                    ToPhoneNumber = NumberAs.CreateString(11),
                    Body = _fixture.Create<string>(),
                    IsCancelled = false,
                });
                uow.Commit();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.Dequeue_TextV1(text.Id);
                result.Should().BeTrue();
            }
        }
    }
}
