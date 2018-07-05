using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class NotifyControllerTest : StartupTest
    {
        private TestServer _owin;

        public NotifyControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_NotifyV1_Email_Fail_InvalidRecipient()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();
            var recipient = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await JwtHelper.GetAccessTokenV2(_ioc, client.Name, audience.Name, user.Email);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var email = new UserCreateEmail()
            {
                FromId = user.Id,
                FromEmail = user.Email,
                FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                ToId = recipient.Id,
                ToEmail = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-" + recipient.Email,
                ToDisplay = string.Format("{0} {1}", recipient.FirstName, recipient.LastName),
                Subject = BaseLib.Statics.ApiUnitTestEmailSubject + "-" + BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4),
                HtmlContent = BaseLib.Statics.ApiUnitTestEmailContent + "-" + BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4)
            };

            var response = await request.PostAsJsonAsync("/notify/v1/email", email);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_NotifyV1_Email_Success()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();
            var recipient = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await JwtHelper.GetAccessTokenV2(_ioc, client.Name, audience.Name, user.Email);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var email = new UserCreateEmail()
            {
                FromId = user.Id,
                FromEmail = user.Email,
                FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                ToId = recipient.Id,
                ToEmail = recipient.Email,
                ToDisplay = string.Format("{0} {1}", recipient.FirstName, recipient.LastName),
                Subject = BaseLib.Statics.ApiUnitTestEmailSubject + "-" + BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4),
                HtmlContent = BaseLib.Statics.ApiUnitTestEmailContent + "-" + BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4)
            };

            var response = await request.PostAsJsonAsync("/notify/v1/email", email);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Admin_NotifyV1_Text_Fail_InvalidRecipient()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();
            var recipient = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await JwtHelper.GetAccessTokenV2(_ioc, client.Name, audience.Name, user.Email);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var email = new UserCreateText()
            {
                FromId = user.Id,
                FromPhoneNumber = user.PhoneNumber,
                ToId = recipient.Id,
                ToPhoneNumber = BaseLib.Helpers.CryptoHelper.CreateRandomNumberAsString(10),
                Body = BaseLib.Statics.ApiUnitTestEmailContent + "-" + BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4)
            };

            var response = await request.PostAsJsonAsync("/notify/v1/text", email);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_NotifyV1_Text_Success()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();
            var recipient = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await JwtHelper.GetAccessTokenV2(_ioc, client.Name, audience.Name, user.Email);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var email = new UserCreateText()
            {
                FromId = user.Id,
                FromPhoneNumber = user.PhoneNumber,
                ToId = recipient.Id,
                ToPhoneNumber = recipient.PhoneNumber,
                Body = BaseLib.Statics.ApiUnitTestEmailContent + "-" + BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4)
            };

            var response = await request.PostAsJsonAsync("/notify/v1/text", email);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
