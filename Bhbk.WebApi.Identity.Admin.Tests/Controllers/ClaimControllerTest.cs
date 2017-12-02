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
        public async Task Api_Admin_Claim_Create_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new ClaimController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var claim = new Claim(BaseLib.Statics.ApiUnitTestClaimType,
                BaseLib.Statics.ApiUnitTestClaimValue + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4));

            var result = await controller.CreateClaim(user.Id, claim) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<Claim>().Subject;

            data.Type.Should().Be(claim.Type);
        }

        [TestMethod]
        public async Task Api_Admin_Claim_Delete_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new ClaimController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var claim = new Claim(BaseLib.Statics.ApiUnitTestClaimType,
                BaseLib.Statics.ApiUnitTestClaimValue + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4));

            var add = await Context.UserMgmt.AddClaimAsync(user.Id, claim);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var find = user.AppUserClaim.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Single();
            var delete = Context.UserMgmt.Store.Mf.Evolve.DoIt(find);
            var result = await controller.DeleteClaim(user.Id, delete) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = user.AppUserClaim.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Claim_Get_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new ClaimController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var result = await controller.GetClaims(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<Claim>>().Subject;

            data.Count().Should().Be(user.AppUserClaim.Count());
        }
    }
}
