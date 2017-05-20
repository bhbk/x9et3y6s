using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests
{
    [TestClass]
    public class AudienceControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public AudienceControllerTest()
        {
            //_owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public void Api_Audience_GetAll_Success()
        {
            var controller = new AudienceController(UoW);
            var result = controller.GetAudiences() as OkNegotiatedContentResult<IEnumerable<AudienceModel.Return.Audience>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<AudienceModel.Return.Audience>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.AudienceRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Audience_Get_Success()
        {
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceRepository.Get().First();
            var result = await controller.GetAudience(audience.Id) as OkNegotiatedContentResult<AudienceModel.Return.Audience>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(AudienceModel.Return.Audience));
            result.Content.Id.Should().Be(audience.Id);
        }

        [TestMethod]
        public async Task Api_Audience_GetRoleList_Success()
        {
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceRepository.Get().First();
            var result = await controller.GetRolesInAudience(audience.Id) as OkNegotiatedContentResult<IEnumerable<RoleModel.Return.Role>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<RoleModel.Return.Role>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Audience_GetKey_Success()
        {
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceRepository.Get().First();
            var result = await controller.GetAudienceKey(audience.Name) as OkNegotiatedContentResult<string>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(string));
            result.Content.ShouldBeEquivalentTo(audience.AudienceKey);
        }

        [TestMethod]
        public async Task Api_Audience_Create_Success()
        {
            string name = "Audience-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new AudienceController(UoW);
            var model = new AudienceModel.Binding.Create()
            {
                ClientId = UoW.ClientRepository.Get().First().Id,
                Name = name,
                AudienceType = BaseLib.AudienceType.ThinClient.ToString(),
                Enabled = true
            };
            var result = await controller.CreateAudience(model) as OkNegotiatedContentResult<AudienceModel.Return.Audience>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(AudienceModel.Return.Audience));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Audience_Update_Success()
        {
            string name = "Audience-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceRepository.Get().First();
            var model = new AudienceModel.Binding.Update()
            {
                Id = audience.Id,
                ClientId = UoW.ClientRepository.Get().First().Id,
                Name = name + "(Updated)",
                Enabled = true
            };
            var result = await controller.UpdateAudience(model.Id, model) as OkNegotiatedContentResult<AudienceModel.Return.Audience>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(AudienceModel.Return.Audience));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Audience_Delete_Success()
        {
            var controller = new AudienceController(UoW);
            var audience = UoW.AudienceRepository.Get().First();

            var result = await controller.DeleteAudience(audience.Id) as OkResult;
            var check = UoW.RoleRepository.Find(audience.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }
    }
}
