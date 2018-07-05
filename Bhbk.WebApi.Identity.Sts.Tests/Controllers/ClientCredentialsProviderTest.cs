using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Providers;
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
using System.Web;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class ClientCredentialsProviderTest : StartupTest
    {
        private TestServer _owin;
        private S2STests _s2s;

        public ClientCredentialsProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _s2s = new S2STests(_conf, _ioc, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_ClientV2_Success()
        {
            //evaluate this unit test further... it is not good yet...
            Assert.Fail();

            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(user.Email, TimeSpan.FromSeconds(10), user));

            var result = await _s2s.ClientCredentialsV2(client.Id.ToString(), code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }
    }
}
