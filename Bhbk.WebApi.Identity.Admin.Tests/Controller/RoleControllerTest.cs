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
    public class RoleControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public RoleControllerTest()
        {
            //_owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public void Api_Role_GetAll_Success()
        {
            var controller = new RoleController(UoW);
            var result = controller.GetRoles() as OkNegotiatedContentResult<IEnumerable<RoleModel.Return.Role>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<RoleModel.Return.Role>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Role_Get_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleRepository.Get().First();
            var result = await controller.GetRole(role.Id) as OkNegotiatedContentResult<RoleModel.Return.Role>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(RoleModel.Return.Role));
            result.Content.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Role_GetUserList_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleRepository.Get().First();
            var result = await controller.GetUsersInRole(role.Id) as OkNegotiatedContentResult<IEnumerable<UserModel.Return.User>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<UserModel.Return.User>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Role_Create_Success()
        {
            string name = "Role-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RoleController(UoW);
            var model = new RoleModel.Binding.Create()
            {
                Name = name,
                AudienceId = UoW.AudienceRepository.Get().First().Id
            };
            var result = await controller.CreateRole(model) as OkNegotiatedContentResult<RoleModel.Return.Role>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(RoleModel.Return.Role));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Role_Update_Success()
        {
            string name = "Role-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RoleController(UoW);
            var role = UoW.RoleRepository.Get().First();
            var model = new RoleModel.Binding.Update()
            {
                Id = role.Id,
                Name = name + "(Updated)",
                Immutable = false,                
                AudienceId = UoW.AudienceRepository.Get().First().Id
            };
            var result = await controller.UpdateRole(model.Id, model) as OkNegotiatedContentResult<RoleModel.Return.Role>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(RoleModel.Return.Role));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Role_Delete_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleRepository.Get().First();
            var result = await controller.DeleteRole(role.Id) as OkResult;
            var check = UoW.RoleRepository.Find(role.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }
    }
}
