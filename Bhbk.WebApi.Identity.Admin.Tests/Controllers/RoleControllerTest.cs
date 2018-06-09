using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Models;
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
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new RoleCreate()
            {
                AudienceId = IoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRoleA + "-" + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await IoC.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(model).Devolve());
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await IoC.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            var result = await controller.AddRoleToUser(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Create_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);
            var model = new RoleCreate()
            {
                AudienceId = IoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRoleA + "-" + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateRole(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Role_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);
            var role = IoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await controller.DeleteRole(role.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = IoC.RoleMgmt.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Role_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);
            var role = IoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await controller.GetRole(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);

            var result = await controller.GetRoles() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<RoleResult>>().Subject;

            data.Count().Should().Equals(IoC.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetUserList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);
            var role = IoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await controller.GetRoleUsers(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<UserResult>>().Subject;

            data.Count().Should().Equals(IoC.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_RemoveFromUser_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new RoleCreate()
            {
                AudienceId = IoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRoleA + "-" + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await IoC.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(model).Devolve());
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await IoC.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            var add = await IoC.UserMgmt.AddToRoleAsync(user, model.Name);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveRoleFromUser(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new RoleController(IoC);
            var role = IoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            var model = new RoleUpdate()
            {
                Id = role.Id,
                AudienceId = IoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRoleA + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateRole(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
