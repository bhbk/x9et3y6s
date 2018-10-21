using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class ClientCredentialsProviderTest : StartupTest
    {
        private TestServer _owin;
        private S2STester _s2s;

        public ClientCredentialsProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _s2s = new S2STester(_conf, _ioc.ContextStatus, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_ClientV1_Fail_NotImplemented()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await _s2s.StsClientCredentialsV2(client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_ClientV2_Fail_ClientNotFound()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await _s2s.StsClientCredentialsV2(Guid.NewGuid().ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_ClientV2_Fail_ClientSecret()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await _s2s.StsClientCredentialsV2(client.Id.ToString(), RandomNumber.CreateBase64(16));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_ClientV2_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await _s2s.StsClientCredentialsV2(client.Id.ToString(), client.ClientKey);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }
    }
}
