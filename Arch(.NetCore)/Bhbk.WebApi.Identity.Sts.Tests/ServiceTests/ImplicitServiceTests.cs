using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;

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
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                var imp = await service.Http.Implicit_AuthV1(
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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = await service.Http.Implicit_AuthV2(
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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = await service.Http.Implicit_AuthV2(
                    new ImplicitV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = client.Id.ToString(),
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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = service.Http.Implicit_AuthV2(
                    new ImplicitV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "implicit",
                        user = user.Id.ToString(),
                        redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    }).Result;
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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = service.Http.Implicit_AuthV2(
                    new ImplicitV2()
                    {
                        issuer = user.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "implicit",
                        user = Guid.NewGuid().ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    }).Result;
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
