using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Me;
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

namespace Bhbk.WebApi.Identity.User.Tests.ServiceTests
{
    public class MOTDServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public MOTDServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Me_MOTDV1_Get_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new UserService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateMOTDs();

                var result = await service.Endpoints.MOTD_GetV1(Base64.CreateString(8));
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task Me_MOTDV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new UserService(conf, env.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, env.InstanceType, owin)
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateMOTDs();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var result = await service.MOTD_GetV1();
                result.Should().BeAssignableTo<MOTDTssV1>();
            }
        }
    }
}
