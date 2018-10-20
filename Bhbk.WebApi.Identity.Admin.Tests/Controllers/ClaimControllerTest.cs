using Bhbk.Lib.Core.Cryptography;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class ClaimControllerTest : StartupTest
    {
        private TestServer _owin;

        public ClaimControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_ClaimV1_Create_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClaimController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var claim = new Claim(BaseLib.Statics.ApiUnitTestClaimType, BaseLib.Statics.ApiUnitTestClaimValue);

            controller.SetUser(user.Id);

            var result = await controller.CreateClaimV1(user.Id, claim) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<Claim>().Subject;

            data.Type.Should().Be(claim.Type);
        }

        [TestMethod]
        public async Task Api_Admin_ClaimV1_Delete_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClaimController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var claim = new Claim(BaseLib.Statics.ApiUnitTestClaimType, 
                BaseLib.Statics.ApiUnitTestClaimValue + "-" + RandomNumber.CreateBase64(4));

            var add = await _ioc.UserMgmt.AddClaimAsync(user, claim);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            controller.SetUser(user.Id);

            var result = await controller.DeleteClaimV1(user.Id, claim) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = user.AppUserClaim.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_ClaimV1_Get_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClaimController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetClaimsV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<Claim>>().Subject;

            data.Count().Should().Be(user.AppUserClaim.Count());
        }
    }
}
