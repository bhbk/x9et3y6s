using Bhbk.Lib.Identity.Factory;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controller
{
    [TestClass]
    public class AudienceControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public AudienceControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Create_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestAudience + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new AudienceController(UoW);
            var model = new AudienceCreate()
            {
                ClientId = UoW.ClientMgmt.Store.Get().First().Id,
                Name = name,
                AudienceType = BaseLib.AudienceType.ThinClient.ToString(),
                AudienceKey = BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(32),
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateAudience(model) as OkNegotiatedContentResult<AudienceModel>;
            result.Content.Should().BeAssignableTo(typeof(AudienceModel));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Delete_Success()
        {
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceMgmt.Store.Get().First();

            var result = await controller.DeleteAudience(audience.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = UoW.AudienceMgmt.Store.Get(x => x.Id == audience.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Get_Success()
        {
            var controller = new AudienceController(UoW);
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var result = await controller.GetAudience(audience.Id) as OkNegotiatedContentResult<AudienceModel>;
            result.Content.Should().BeAssignableTo(typeof(AudienceModel));
            result.Content.Id.Should().Be(audience.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetList_Success()
        {
            var controller = new AudienceController(UoW);

            var result = await controller.GetAudiences() as OkNegotiatedContentResult<IList<AudienceModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<AudienceModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.AudienceMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetRoleList_Success()
        {
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceMgmt.Store.Get().First();

            var result = await controller.GetAudienceRoles(audience.Id) as OkNegotiatedContentResult<IList<RoleModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<RoleModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Update_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestAudience + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceMgmt.Store.Get().First();
            var model = new AudienceUpdate()
            {
                Id = audience.Id,
                ClientId = UoW.ClientMgmt.Store.Get().First().Id,
                Name = name + "(Updated)",
                AudienceType = audience.AudienceType,
                AudienceKey = audience.AudienceKey,
                Enabled = true
            };

            var result = await controller.UpdateAudience(model.Id, model) as OkNegotiatedContentResult<AudienceModel>;
            result.Content.Should().BeAssignableTo(typeof(AudienceModel));
            result.Content.Name.Should().Be(model.Name);
        }
    }
}
