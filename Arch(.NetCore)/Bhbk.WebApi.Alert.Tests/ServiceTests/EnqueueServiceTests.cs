using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    public class EnqueueServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public EnqueueServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Alert_EnqueueV1_Email_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin)
                };

                var result = await service.Endpoints.Enqueue_EmailV1(Base64.CreateString(8), new EmailV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Alert).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Enqueue_EmailV1(rop.RawData, new EmailV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Alert_EnqueueV1_Email_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Alert).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();
                var result = await service.Enqueue_EmailV1(
                    new EmailV1()
                    {
                        FromEmail = user.EmailAddress,
                        ToEmail = testUser.EmailAddress,
                        Subject = Constants.TestEmailSubject + "-" + Base64.CreateString(4),
                        Body = Constants.TestEmailContent + "-" + Base64.CreateString(4)
                    });
                result.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Alert_EnqueueV1_Text_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin)
                };

                var result = await service.Endpoints.Enqueue_TextV1(Base64.CreateString(8), new TextV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Alert).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                result = await service.Endpoints.Enqueue_TextV1(rop.RawData, new TextV1());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Alert_EnqueueV1_Text_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AlertService(instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Alert).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                user.PhoneNumber = NumberAs.CreateString(11);
                user.PhoneNumberConfirmed = true;

                uow.Users.Update(user);
                uow.Commit();

                var testUser = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();
                var result = await service.Enqueue_TextV1(
                    new TextV1()
                    {
                        FromPhoneNumber = user.PhoneNumber,
                        ToPhoneNumber = testUser.PhoneNumber,
                        Body = Constants.TestTextContent,
                    });
                result.Should().BeTrue();
            }
        }
    }
}
