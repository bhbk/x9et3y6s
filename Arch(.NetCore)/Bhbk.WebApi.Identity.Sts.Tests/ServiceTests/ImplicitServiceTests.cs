using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_TBL;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_TBL;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    public class ImplicitServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public ImplicitServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_ImplicitV1_Auth_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                var imp = await service.Endpoints.Implicit_AuthV1(
                    new ImplicitV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "implicit",
                        username = Guid.NewGuid().ToString(),
                        redirect_uri = Base64.CreateString(8),
                        response_type = "token",
                        scope = Base64.CreateString(8),
                        state = Base64.CreateString(8),
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_ClientNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                var data = new TestDataFactory_TBL(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var url = new Uri(Constants.TestUriLink);
                var state = Base64.CreateString(8);
                var imp = await service.Endpoints.Implicit_AuthV2(
                    new ImplicitV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "implicit",
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_IssuerNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                var data = new TestDataFactory_TBL(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var url = new Uri(Constants.TestUriLink);
                var state = Base64.CreateString(8);
                var imp = await service.Endpoints.Implicit_AuthV2(
                    new ImplicitV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "implicit",
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_UrlNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                var data = new TestDataFactory_TBL(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var url = new Uri(Constants.TestUriLink);
                var state = Base64.CreateString(8);
                var imp = await service.Endpoints.Implicit_AuthV2(
                    new ImplicitV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "implicit",
                        user = user.Id.ToString(),
                        redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_UserNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                var data = new TestDataFactory_TBL(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var url = new Uri(Constants.TestUriLink);
                var state = Base64.CreateString(8);
                var imp = await service.Endpoints.Implicit_AuthV2(
                    new ImplicitV2()
                    {
                        issuer = user.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "implicit",
                        user = Guid.NewGuid().ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
