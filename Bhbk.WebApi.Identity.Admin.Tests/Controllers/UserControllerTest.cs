using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class UserControllerTest : StartupTest
    {
        private TestServer _owin;

        public UserControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_User_Create_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var model = new UserCreate()
            {
                Email = BaseLib.Statics.ApiUnitTestUserEmail + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4) + ".net",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            var result = await controller.CreateUser(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

            data.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_User_DeleteImmutable_Fail()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            await Context.UserMgmt.Store.SetImmutableEnabledAsync(user, true);

            var result = await controller.DeleteUser(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = Context.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_DeleteMutable_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var result = await controller.DeleteUser(user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = Context.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_Get_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var result = await controller.GetUser(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);

            var result = await controller.GetUsers() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<UserModel>>().Subject;

            data.Count().Should().Equals(Context.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetByName_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var result = await controller.GetUserByName(user.UserName) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetLoginList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var result = await controller.GetUserLogins(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<string>>().Subject;

            data.Count().Should().Equals(Context.LoginMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetRoleList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var result = await controller.GetUserRoles(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<string>>().Subject;

            data.Count().Should().Equals(Context.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Fail()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var remove = await Context.UserMgmt.RemovePasswordAsync(user.Id);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPasswordConfirm = EntrophyHelper.GenerateRandomBase64(16)
            };

            var result = await controller.AddPassword(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await Context.UserMgmt.HasPasswordAsync(user.Id);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var remove = await Context.UserMgmt.RemovePasswordAsync(user.Id);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.AddPassword(user.Id, model) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await Context.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Fail()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var remove = await Context.UserMgmt.RemovePasswordAsync(user.Id);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var result = await controller.RemovePassword(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await Context.UserMgmt.HasPasswordAsync(user.Id);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            var result = await controller.RemovePassword(user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await Context.UserMgmt.HasPasswordAsync(user.Id);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Fail()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var model = new UserAddPassword()
            {
                NewPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPasswordConfirm = EntrophyHelper.GenerateRandomBase64(16)
            };

            var result = await controller.ResetPassword(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await Context.UserMgmt.CheckPasswordAsync(user.Id, model.NewPassword);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.ResetPassword(user.Id, model) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await Context.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_Update_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string email = BaseLib.Statics.ApiUnitTestUserEmail + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new UserController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var model = new UserUpdate()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)",
                LockoutEnabled = false,
                LockoutEnd = DateTime.Now.AddDays(30)
            };

            var result = await controller.UpdateUser(model.Id, model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
