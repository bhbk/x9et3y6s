using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helper;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controller
{
    [TestClass]
    public class UserControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public UserControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Admin_User_Create_Success()
        {
            var controller = new UserController(UoW);
            var model = new UserCreate()
            {
                Email = BaseLib.Statics.ApiUnitTestUserEmail + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            var result = await controller.CreateUser(model) as OkNegotiatedContentResult<UserModel>;
            result.Content.Should().BeAssignableTo(typeof(UserModel));
            result.Content.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_User_DeleteImmutable_Fail()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            await UoW.UserMgmt.Store.SetImmutableEnabledAsync(user, true);

            var result = await controller.DeleteUser(user.Id) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));

            var check = UoW.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_DeleteMutable_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var result = await controller.DeleteUser(user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = UoW.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_Get_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var result = await controller.GetUser(user.Id) as OkNegotiatedContentResult<UserModel>;
            result.Content.Should().BeAssignableTo(typeof(UserModel));
            result.Content.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetList_Success()
        {
            var controller = new UserController(UoW);

            var result = await controller.GetUsers() as OkNegotiatedContentResult<IList<UserModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<UserModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetByName_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var result = await controller.GetUserByName(user.UserName) as OkNegotiatedContentResult<UserModel>;
            result.Content.Should().BeAssignableTo(typeof(UserModel));
            result.Content.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetProviderList_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var result = await controller.GetUserProviders(user.Id) as OkNegotiatedContentResult<IList<ProviderModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<ProviderModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.ProviderMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetRoleList_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var result = await controller.GetUserRoles(user.Id) as OkNegotiatedContentResult<IList<string>>;
            result.Content.Should().BeAssignableTo(typeof(IList<string>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Fail()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var remove = await UoW.UserMgmt.RemovePasswordAsync(user.Id);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPasswordConfirm = EntrophyHelper.GenerateRandomBase64(16)
            };

            var result = await controller.AddPassword(user.Id, model) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));

            var check = await UoW.UserMgmt.HasPasswordAsync(user.Id);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var remove = await UoW.UserMgmt.RemovePasswordAsync(user.Id);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.AddPassword(user.Id, model) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await UoW.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Fail()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var remove = await UoW.UserMgmt.RemovePasswordAsync(user.Id);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var result = await controller.RemovePassword(user.Id) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));

            var check = await UoW.UserMgmt.HasPasswordAsync(user.Id);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            var result = await controller.RemovePassword(user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await UoW.UserMgmt.HasPasswordAsync(user.Id);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Fail()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var model = new UserAddPassword()
            {
                NewPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPasswordConfirm = EntrophyHelper.GenerateRandomBase64(16)
            };

            var result = await controller.ResetPassword(user.Id, model) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));

            var check = await UoW.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.ResetPassword(user.Id, model) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await UoW.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_Update_Success()
        {
            string email = BaseLib.Statics.ApiUnitTestUserEmail + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new UserController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var model = new UserUpdate()
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = "FirstName",
                LastName = "LastName",
                LockoutEnabled = false,
                LockoutEndDateUtc = DateTime.Now.AddDays(30),
                TwoFactorEnabled = false,
                Immutable = false
            };

            var result = await controller.UpdateUser(model.Id, model) as OkNegotiatedContentResult<UserModel>;
            result.Content.Should().BeAssignableTo(typeof(UserModel));
            result.Content.Email.Should().Be(model.Email);
        }
    }
}
