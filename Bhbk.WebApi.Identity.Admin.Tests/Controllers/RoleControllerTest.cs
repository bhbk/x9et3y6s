using Bhbk.Lib.Identity.Factory;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class RoleControllerTest : StartupTest
    {
        private TestServer _owin;

        public RoleControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_Role_AddToUser_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new RoleController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var model = new RoleCreate()
            {
                AudienceId = Context.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await Context.RoleMgmt.CreateAsync(Context.RoleMgmt.Store.Mf.Create.DoIt(model));
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await Context.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(RoleModel));

            var result = await controller.AddRoleToUser(role.Id, user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Create_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RoleController(Context);
            var model = new RoleCreate()
            {
                AudienceId = Context.AudienceMgmt.Store.Get().First().Id,
                Name = name,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateRole(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleModel>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Role_Delete_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new RoleController(Context);
            var role = Context.RoleMgmt.Store.Get().First();

            var result = await controller.DeleteRole(role.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = Context.RoleMgmt.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Role_Get_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new RoleController(Context);
            var role = Context.RoleMgmt.Store.Get().First();

            var result = await controller.GetRole(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleModel>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new RoleController(Context);

            var result = await controller.GetRoles() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<RoleModel>>().Subject;

            data.Count().Should().Equals(Context.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetUserList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new RoleController(Context);
            var role = Context.RoleMgmt.Store.Get().First();

            var result = await controller.GetRoleUsers(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<UserModel>>().Subject;

            data.Count().Should().Equals(Context.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_RemoveFromUser_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new RoleController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var model = new RoleCreate()
            {
                AudienceId = Context.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await Context.RoleMgmt.CreateAsync(Context.RoleMgmt.Store.Mf.Create.DoIt(model));
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await Context.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(RoleModel));

            var add = await Context.UserMgmt.AddToRoleAsync(user.Id, model.Name);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveRoleFromUser(role.Id, user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Update_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string name = BaseLib.Statics.ApiUnitTestRole + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RoleController(Context);
            var role = Context.RoleMgmt.Store.Get().First();
            var model = new RoleUpdate()
            {
                Id = role.Id,
                AudienceId = Context.AudienceMgmt.Store.Get().First().Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateRole(model.Id, model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleModel>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
