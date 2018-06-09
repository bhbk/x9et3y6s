using Bhbk.Lib.Identity.Factory;
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
        public async Task Api_Admin_User_Create_Fail_InvalidEmail()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var model = new UserCreate()
            {
                Email = BaseLib.Statics.ApiUnitTestUserA + "-" + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4),
                FirstName = "FirstName",
                LastName = "LastName",
                PhoneNumber = "0123456789",
                LockoutEnabled = false,
                Immutable = false,
            };

            var result = await controller.CreateUser(model) as BadRequestResult;
            result.Should().BeAssignableTo(typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Api_Admin_User_Create_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var model = new UserCreate()
            {
                Email = BaseLib.Statics.ApiUnitTestUserA + "-" + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4),
                FirstName = "FirstName",
                LastName = "LastName",
                PhoneNumber = "0123456789",
                LockoutEnabled = false,
                Immutable = false,
            };

            var result = await controller.CreateUser(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_User_DeleteImmutable_Fail()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            await IoC.UserMgmt.Store.SetImmutableEnabledAsync(user, true);

            var result = await controller.DeleteUser(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = IoC.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_DeleteMutable_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.DeleteUser(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = IoC.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUser(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);

            var result = await controller.GetUsers() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<UserResult>>().Subject;

            data.Count().Should().Equals(IoC.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetByName_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUser(user.UserName) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetAudienceList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserAudiences(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<AudienceResult>>().Subject;

            data.Count().Should().Equals(IoC.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetLoginList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserLogins(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<LoginResult>>().Subject;

            data.Count().Should().Equals(IoC.LoginMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetRoleList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserRoles(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<RoleResult>>().Subject;

            data.Count().Should().Equals(IoC.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await IoC.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(16),
                NewPasswordConfirm = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(16)
            };

            var result = await controller.AddPassword(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await IoC.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await IoC.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.AddPassword(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await IoC.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await IoC.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var result = await controller.RemovePassword(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await IoC.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.RemovePassword(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await IoC.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(16),
                NewPasswordConfirm = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(16)
            };

            var result = await controller.ResetPassword(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await IoC.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.ResetPassword(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await IoC.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new UserController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserUpdate()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)",
                LockoutEnabled = false,
                LockoutEnd = DateTime.Now.AddDays(30)
            };

            var result = await controller.UpdateUser(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
