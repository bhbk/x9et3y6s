using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
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
            var user = UoW.UserRepository.Get().First();
            var claim = new UserClaimModel.Binding.Create()
            {
                UserId = user.Id,
                ClaimType = BaseLib.Statics.ApiUnitTestsClaimType,
                ClaimValue = BaseLib.Statics.ApiUnitTestsClaimValue + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Immutable = false
            };
            var result = await controller.CreateClaim(user.Id, claim) as OkNegotiatedContentResult<UserClaimModel.Return.Claim>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserClaimModel.Return.Claim));
            result.Content.ClaimType.Should().BeEquivalentTo(claim.ClaimType);
        }

        [TestMethod]
        public async Task Api_Admin_Claim_Delete_Success()
        {
            var controller = new ClaimController(UoW);
            var user = UoW.UserRepository.Get().First();
            var claim = user.Claims.First();
            var result = await controller.DeleteClaim(user.Id, claim.Id) as OkResult;
            var check = UoW.UserRepository.Find(user.Id).Claims.Where(x => x.ClaimType == claim.ClaimType && x.ClaimValue == claim.ClaimValue).Any();

            result.Should().NotBeNull();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Claim_Get_Success()
        {
            var controller = new ClaimController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetClaims(user.Id) as OkNegotiatedContentResult<IEnumerable<UserClaimModel.Return.Claim>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<UserClaimModel.Return.Claim>));
            result.Content.Count().ShouldBeEquivalentTo(user.Claims.Count());
        }

        [TestMethod]
        public async Task Api_Admin_Claim_GetAll_Success()
        {
            var controller = new ClaimController(UoW);
            var result = await controller.GetClaims() as IHttpActionResult;

            result.Should().NotBeNull();
        }
    }
}
