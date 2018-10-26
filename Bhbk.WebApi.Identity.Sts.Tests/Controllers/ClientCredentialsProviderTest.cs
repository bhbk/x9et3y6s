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
        public async Task Api_Sts_OAuth_ClientV1_Fail_NotImplemented()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_ClientV2_Fail_ClientNotFound()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(Guid.NewGuid().ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod, Ignore]
        public async Task Api_Sts_OAuth_ClientV2_Fail_ClientSecret()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(client.Id.ToString(), RandomValues.CreateBase64String(16));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod, Ignore]
        public async Task Api_Sts_OAuth_ClientV2_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await _sts.ClientCredentialsV2(client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }
    }
}
