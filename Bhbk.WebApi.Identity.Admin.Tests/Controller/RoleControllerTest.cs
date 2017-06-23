using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.AspNet.Identity;
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
    public class RoleControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public RoleControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Admin_Role_AddToUser_Success()
        {
            var controller = new RoleController(UoW);
            var user = UoW.UserMgmt.LocalStore.Get().First();
            var model = new RoleModel.Create()
            {
                AudienceId = UoW.AudienceMgmt.LocalStore.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await UoW.RoleMgmt.CreateAsync(model);
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await UoW.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(RoleModel.Model));

            var result = await controller.AddRoleToUser(role.Id, user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Create_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RoleController(UoW);
            var model = new RoleModel.Create()
            {
                AudienceId = UoW.AudienceMgmt.LocalStore.Get().First().Id,
                Name = name,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateRole(model) as OkNegotiatedContentResult<RoleModel.Model>;
            result.Content.Should().BeAssignableTo(typeof(RoleModel.Model));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Role_Delete_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleMgmt.LocalStore.Get().First();

            var result = await controller.DeleteRole(role.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = UoW.RoleMgmt.LocalStore.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Role_Get_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleMgmt.LocalStore.Get().First();

            var result = await controller.GetRole(role.Id) as OkNegotiatedContentResult<RoleModel.Model>;
            result.Content.Should().BeAssignableTo(typeof(RoleModel.Model));
            result.Content.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetList_Success()
        {
            var controller = new RoleController(UoW);

            var result = await controller.GetRoles() as OkNegotiatedContentResult<IList<RoleModel.Model>>;
            result.Content.Should().BeAssignableTo(typeof(IList<RoleModel.Model>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleMgmt.LocalStore.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetUserList_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleMgmt.LocalStore.Get().First();

            var result = await controller.GetRoleUsers(role.Id) as OkNegotiatedContentResult<IList<UserModel.Model>>;
            result.Content.Should().BeAssignableTo(typeof(IList<UserModel.Model>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserMgmt.LocalStore.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_RemoveFromUser_Success()
        {
            var controller = new RoleController(UoW);
            var user = UoW.UserMgmt.LocalStore.Get().First();
            var model = new RoleModel.Create()
            {
                AudienceId = UoW.AudienceMgmt.LocalStore.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await UoW.RoleMgmt.CreateAsync(model);
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await UoW.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(RoleModel.Model));

            var add = await UoW.UserMgmt.AddToRoleAsync(user.Id, model.Name);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveRoleFromUser(role.Id, user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Update_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RoleController(UoW);
            var role = UoW.RoleMgmt.LocalStore.Get().First();
            var model = new RoleModel.Update()
            {
                Id = role.Id,
                AudienceId = UoW.AudienceMgmt.LocalStore.Get().First().Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateRole(model.Id, model) as OkNegotiatedContentResult<RoleModel.Model>;
            result.Content.Should().BeAssignableTo(typeof(RoleModel.Model));
            result.Content.Name.Should().Be(model.Name);
        }
    }
}
