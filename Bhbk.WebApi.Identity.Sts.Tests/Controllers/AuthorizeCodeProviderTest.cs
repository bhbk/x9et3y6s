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
    public class AuthorizeCodeProviderTest : StartupTest
    {
        private TestServer _owin;
        private S2STests _s2s;

        public AuthorizeCodeProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _s2s = new S2STests(_conf, _ioc, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_AuthorizeV2_Fail_ClientInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var redirect = string.Format("{0}{1}{2}", _conf["IdentitySpas:MeUrl"], _conf["IdentitySpas:MePath"], "/unit-tests");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(10), user));

            var result = await _s2s.AuthorizeCodeV2(Guid.NewGuid().ToString(), user.Id.ToString(), redirect, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AuthorizeV2_Fail_CodeInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var redirect = string.Format("{0}{1}{2}", _conf["IdentitySpas:MeUrl"], _conf["IdentitySpas:MePath"], "/unit-tests");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(10), user));

            var result = await _s2s.AuthorizeCodeV2(client.Id.ToString(), user.Id.ToString(), redirect, BaseLib.Helpers.CryptoHelper.CreateRandomBase64(64));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AuthorizeV2_Fail_UserInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var redirect = string.Format("{0}{1}{2}", _conf["IdentitySpas:MeUrl"], _conf["IdentitySpas:MePath"], "/unit-tests");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(10), user));

            var result = await _s2s.AuthorizeCodeV2(client.Id.ToString(), Guid.NewGuid().ToString(), redirect, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AuthorizeV2_Success()
        {
            _data.Destroy();
            _data.CreateTest();
            
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var redirect = string.Format("{0}{1}{2}", _conf["IdentitySpas:MeUrl"], _conf["IdentitySpas:MePath"], "/unit-tests");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(10), user));

            var result = await _s2s.AuthorizeCodeV2(client.Id.ToString(), user.Id.ToString(), redirect, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();

            check = JwtHelper.IsValidJwtFormat(refresh);
            check.Should().BeTrue();
        }
    }
}
