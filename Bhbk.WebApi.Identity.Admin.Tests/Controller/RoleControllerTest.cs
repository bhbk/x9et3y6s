using Bhbk.Lib.Identity.Factory;
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
            var user = UoW.UserMgmt.Store.Get().First();
            var model = new RoleCreate()
            {
                AudienceId = UoW.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await UoW.RoleMgmt.CreateAsync(UoW.RoleMgmt.Store.Mf.Create.DoIt(model));
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await UoW.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(RoleModel));

            var result = await controller.AddRoleToUser(role.Id, user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Create_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RoleController(UoW);
            var model = new RoleCreate()
            {
                AudienceId = UoW.AudienceMgmt.Store.Get().First().Id,
                Name = name,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateRole(model) as OkNegotiatedContentResult<RoleModel>;
            result.Content.Should().BeAssignableTo(typeof(RoleModel));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Role_Delete_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleMgmt.Store.Get().First();

            var result = await controller.DeleteRole(role.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = UoW.RoleMgmt.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Role_Get_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleMgmt.Store.Get().First();

            var result = await controller.GetRole(role.Id) as OkNegotiatedContentResult<RoleModel>;
            result.Content.Should().BeAssignableTo(typeof(RoleModel));
            result.Content.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetList_Success()
        {
            var controller = new RoleController(UoW);

            var result = await controller.GetRoles() as OkNegotiatedContentResult<IList<RoleModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<RoleModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetUserList_Success()
        {
            var controller = new RoleController(UoW);
            var role = UoW.RoleMgmt.Store.Get().First();

            var result = await controller.GetRoleUsers(role.Id) as OkNegotiatedContentResult<IList<UserModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<UserModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_RemoveFromUser_Success()
        {
            var controller = new RoleController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var model = new RoleCreate()
            {
                AudienceId = UoW.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await UoW.RoleMgmt.CreateAsync(UoW.RoleMgmt.Store.Mf.Create.DoIt(model));
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await UoW.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(RoleModel));

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
            var role = UoW.RoleMgmt.Store.Get().First();
            var model = new RoleUpdate()
            {
                Id = role.Id,
                AudienceId = UoW.AudienceMgmt.Store.Get().First().Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateRole(model.Id, model) as OkNegotiatedContentResult<RoleModel>;
            result.Content.Should().BeAssignableTo(typeof(RoleModel));
            result.Content.Name.Should().Be(model.Name);
        }
    }
}
