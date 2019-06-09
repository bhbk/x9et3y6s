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
        private readonly IConfiguration _conf;
        private readonly IMapper _mapper;
        private readonly BaseServiceTests _factory;
        private readonly StsService _service;

        public ImplicitServiceTests(BaseServiceTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
            _service = new StsService(_conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV1_Auth_NotImplemented()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var imp = await _service.Http.Implicit_AuthV1(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = await _service.Http.Implicit_AuthV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = await _service.Http.Implicit_AuthV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = _service.Http.Implicit_AuthV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);
                var state = Base64.CreateString(8);
                var imp = _service.Http.Implicit_AuthV2(
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
