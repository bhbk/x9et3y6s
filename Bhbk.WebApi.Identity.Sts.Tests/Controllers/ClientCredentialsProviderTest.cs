using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class ClientCredentialsProviderTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly StsClient _sts;

        public ClientCredentialsProviderTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
            _sts = new StsClient(_conf, _uow.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_IssuerV1_Fail_NotImplemented()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(issuer.Id.ToString(), client.Id.ToString(), issuer.IssuerKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_IssuerV2_Fail_IssuerNotFound()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(Guid.NewGuid().ToString(), client.Id.ToString(), issuer.IssuerKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Not Implemented")]
        public async Task Sts_OAuth2_IssuerV2_Fail_IssuerSecret()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(issuer.Id.ToString(), client.Id.ToString(), RandomValues.CreateBase64String(16));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact(Skip = "Not Implemented")]
        public async Task Sts_OAuth2_IssuerV2_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(issuer.Id.ToString(), client.Id.ToString(), issuer.IssuerKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }
    }
}
