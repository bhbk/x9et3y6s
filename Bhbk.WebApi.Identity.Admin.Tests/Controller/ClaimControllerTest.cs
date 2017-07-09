using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controller
{
    [TestClass]
    public class ClaimControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public ClaimControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Admin_Claim_Create_Success()
        {
            var controller = new ClaimController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var claim = new Claim(BaseLib.Statics.ApiUnitTestClaimType,
                BaseLib.Statics.ApiUnitTestClaimValue + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4));

            var result = await controller.CreateClaim(user.Id, claim) as OkNegotiatedContentResult<Claim>;
            result.Content.Should().BeAssignableTo(typeof(Claim));
            result.Content.Type.Should().BeEquivalentTo(claim.Type);
            result.Content.Value.Should().BeEquivalentTo(claim.Value);
        }

        [TestMethod]
        public async Task Api_Admin_Claim_Delete_Success()
        {
            var controller = new ClaimController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var claim = new Claim(BaseLib.Statics.ApiUnitTestClaimType, 
                BaseLib.Statics.ApiUnitTestClaimValue + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4));

            var add = await UoW.UserMgmt.AddClaimAsync(user.Id, claim);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var find = user.Claims.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Single();
            var delete = UoW.UserMgmt.Store.Mf.Evolve.DoIt(find);
            var result = await controller.DeleteClaim(user.Id, delete) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = user.Claims.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Claim_Get_Success()
        {
            var controller = new ClaimController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var result = await controller.GetClaims(user.Id) as OkNegotiatedContentResult<IList<Claim>>;
            result.Content.Should().BeAssignableTo(typeof(IList<Claim>));
            result.Content.Count().ShouldBeEquivalentTo(user.Claims.Count());
        }
    }
}
