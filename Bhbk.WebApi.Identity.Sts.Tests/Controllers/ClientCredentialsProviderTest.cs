using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Primitives;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class ClientCredentialsProviderTest : StartupTest
    {
        private TestServer _owin;
        private StsTester _sts;

        public ClientCredentialsProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _sts = new StsTester(_conf, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_IssuerV1_Fail_NotImplemented()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(issuer.Id.ToString(), client.Id.ToString(), issuer.IssuerKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_IssuerV2_Fail_IssuerNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(Guid.NewGuid().ToString(), client.Id.ToString(), issuer.IssuerKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod, Ignore]
        public async Task Api_Sts_OAuth2_IssuerV2_Fail_IssuerSecret()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(issuer.Id.ToString(), client.Id.ToString(), RandomValues.CreateBase64String(16));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod, Ignore]
        public async Task Api_Sts_OAuth2_IssuerV2_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(issuer.Id.ToString(), client.Id.ToString(), issuer.IssuerKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }
    }
}
