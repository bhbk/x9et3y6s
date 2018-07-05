using Bhbk.Lib.Identity.Helpers;
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
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class CodeControllerTest : StartupTest
    {
        private TestServer _owin;
        private S2STests _s2s;

        public CodeControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _s2s = new S2STests(_conf, _ioc, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_CodeV2_Fail_ClientInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = Guid.NewGuid();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var redirect = string.Format("{0}{1}{2}", _conf["IdentitySpas:MeUrl"], _conf["IdentitySpas:MePath"], "/unit-tests");
            var state = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(32);

            var result = await _s2s.AuthorizeCodeRequestV2(client.ToString(), user.Id.ToString(), redirect, "all", state);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_CodeV2_Fail_UserInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = Guid.NewGuid();

            var redirect = string.Format("{0}{1}{2}", _conf["IdentitySpas:MeUrl"], _conf["IdentitySpas:MePath"], "/unit-tests");
            var state = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(32);

            var result = await _s2s.AuthorizeCodeRequestV2(client.Id.ToString(), user.ToString(), redirect, "all", state);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_CodeV2_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var redirect = string.Format("{0}{1}{2}", _conf["IdentitySpas:MeUrl"], _conf["IdentitySpas:MePath"], "/unit-tests");
            var state = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(32);

            var result = await _s2s.AuthorizeCodeRequestV2(client.Id.ToString(), user.Id.ToString(), redirect, "all", state);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var check = new Uri(await result.Content.ReadAsStringAsync());
            check.Should().BeAssignableTo(typeof(Uri));
        }
    }
}
