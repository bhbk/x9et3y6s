using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Alert;
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
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    public class EmailServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public EmailServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async ValueTask Alert_EmailV1_Enqueue_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var factory = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AlertService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Enqueue_EmailV1(Base64.CreateString(8), new EmailCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = factory.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, uow.Issuers.Salt, new List<string>() { client.Name }, rop_claims);

                result = await service.Http.Enqueue_EmailV1(rop.RawData, new EmailCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var factory = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AlertService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = factory.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, uow.Issuers.Salt, new List<string>() { client.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var result = await service.Http.Enqueue_EmailV1(rop.RawData,
                    new EmailCreate()
                    {
                        FromId = Guid.NewGuid(),
                        FromEmail = user.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = FakeConstants.ApiTestEmailSubject + "-" + Base64.CreateString(4),
                        HtmlContent = FakeConstants.ApiTestEmailContent + "-" + Base64.CreateString(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var factory = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AlertService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = factory.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, uow.Issuers.Salt, new List<string>() { client.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var result = await service.Http.Enqueue_EmailV1(rop.RawData,
                    new EmailCreate()
                    {
                        FromId = user.Id,
                        FromEmail = testUser.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = FakeConstants.ApiTestEmailSubject + "-" + Base64.CreateString(4),
                        HtmlContent = FakeConstants.ApiTestEmailContent + "-" + Base64.CreateString(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async ValueTask Alert_EmailV1_Enqueue_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var factory = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new AlertService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = factory.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, uow.Issuers.Salt, new List<string>() { client.Name }, rop_claims);

                var testUser = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var result = await service.Http.Enqueue_EmailV1(rop.RawData,
                    new EmailCreate()
                    {
                        FromId = user.Id,
                        FromEmail = user.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = FakeConstants.ApiTestEmailSubject + "-" + Base64.CreateString(4),
                        HtmlContent = FakeConstants.ApiTestEmailContent + "-" + Base64.CreateString(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
